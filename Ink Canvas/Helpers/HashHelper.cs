using System;
using System.Security.Cryptography;
using System.Text;

namespace Ink_Canvas.Helpers
{
    /// <summary>
    /// 哈希计算辅助类，用于路径/标识等短字符串的 MD5 前缀哈希。
    /// </summary>
    internal static class HashHelper
    {
        /// <summary>
        /// 对给定路径字符串计算 MD5 哈希，返回前 8 位十六进制字符串。
        /// </summary>
        /// <param name="filePath">文件路径或任意字符串</param>
        /// <returns>8 位十六进制字符串；异常或空输入时返回 "error" 或 "unknown"</returns>
        public static string GetFileHash(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return "unknown";
                // Persisted PPT folder identifiers depend on this MD5 prefix; this is not a security hash.
                byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(filePath));
                return Convert.ToHexString(hash.AsSpan(0, 4));
            }
            catch (Exception ex)
            {
                LogHelper.WriteLogToFile($"计算文件哈希失败: {ex}", LogHelper.LogType.Error);
                return "error";
            }
        }
    }
}
