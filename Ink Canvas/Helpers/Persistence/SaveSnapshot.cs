using Ink_Canvas.UInk;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Ink_Canvas.Helpers.Persistence
{
    /// <summary>Serialized UI snapshot. Write performs only file IO and compression.</summary>
    public sealed class SaveSnapshot : IDisposable
    {
        public string Path { get; set; }
        public string SuccessMessage { get; set; }
        public bool IsZip { get; set; }
        public List<(string name, byte[] bytes)> Files { get; } = new List<(string, byte[])>();
        public UInkDocument UInkDocument { get; set; }
        public List<(string entryPath, string sourceFile)> Resources { get; } = new List<(string, string)>();
        private readonly List<FileStream> _mediaPins = new List<FileStream>();

        // Deny writes/deletion until SaveFull has staged the media. Never copy large media on the UI thread.
        public void PinMedia(string path)
            => _mediaPins.Add(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));

        public void Write()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            if (UInkDocument != null)
            {
                UInkSaveService.SaveFull(UInkDocument, Path, Resources);
            }
            else if (IsZip)
            {
                AtomicWrite(Path, stream =>
                {
                    using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
                    foreach (var (name, bytes) in Files)
                    {
                        using var entry = archive.CreateEntry(name, CompressionLevel.Optimal).Open();
                        entry.Write(bytes, 0, bytes.Length);
                    }
                });
            }
            else
            {
                foreach (var (name, bytes) in Files)
                    AtomicWrite(name, stream => stream.Write(bytes, 0, bytes.Length));
            }
        }

        public static void AtomicWrite(string path, Action<Stream> write)
        {
            string temporary = path + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write)) write(stream);
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            catch
            {
                try { if (File.Exists(temporary)) File.Delete(temporary); } catch { }
                throw;
            }
        }

        public void Dispose()
        {
            foreach (var pin in _mediaPins) pin.Dispose();
            _mediaPins.Clear();
        }
    }
}
