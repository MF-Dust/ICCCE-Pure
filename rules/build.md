# 编译规范

## 约定

- 下文所有路径均**相对于仓库根目录**（即 `Ink Canvas.sln` 所在目录），命令请在仓库根目录下执行。
- 当前开发分支：**net6**（使用 Augment MCP 检索代码时务必指定该分支，否则调用失败）。
- `dotnet` 已在 PATH 中，直接用 `dotnet` 即可，不需要写全路径。

## 编译命令

### 1. 主解决方案（默认，Debug x64）

```powershell
dotnet build "Ink Canvas.sln" -c Debug -p:Platform=x64
```

**默认配置为 `Debug` + `x64`，任何代码修改完成后都要跑这条命令做构建校验。**

### 2. 核心回归检查（Windows，无需签名证书）

```powershell
dotnet run --project InkCanvas.Core.Tests -c Release
```

PPT 联动仅使用主程序内的 ROT/Office COM；无需额外加载项、运行时或签名证书。涉及 PPT 时，除上述构建和核心测试外，还应实机验证 PowerPoint/WPS 放映、翻页、关闭/重连及智慧模式视频区域。

### 3. 编译单个子项目

```powershell
dotnet build "InkCanvas.Controls\InkCanvas.Controls.csproj"
dotnet build "InkCanvas.SettingsTreeView\InkCanvas.SettingsTreeView.csproj"
```

## 项目列表

### 解决方案内（6 个）

| 项目 | csproj 路径 | 目标框架 | sln 平台映射 |
| --- | --- | --- | --- |
| InkCanvasForClass（主应用） | `Ink Canvas/InkCanvasForClass.csproj` | net10.0-windows10.0.19041.0 | `Debug\|x64` → `Debug\|x64`；`Release\|x64` → `Release\|Any CPU` |
| InkCanvas.Controls | `InkCanvas.Controls/InkCanvas.Controls.csproj` | net10.0-windows10.0.19041.0 | 全部 Any CPU |
| InkCanvas.SettingsTreeView | `InkCanvas.SettingsTreeView/InkCanvas.SettingsTreeView.csproj` | net10.0-windows10.0.19041.0 | 全部 Any CPU |
| InkCanvas.IACoreHelper | `InkCanvas.IACoreHelper/InkCanvas.IACoreHelper.csproj` | net472 | **所有配置一律映射到 x86** |
| InkCanvas.NativeInk.Tests | `InkCanvas.NativeInk.Tests/InkCanvas.NativeInk.Tests.csproj` | net10.0-windows10.0.19041.0 | 全部 Any CPU |
| InkCanvas.Core.Tests | `InkCanvas.Core.Tests/InkCanvas.Core.Tests.csproj` | net10.0-windows10.0.19041.0 | 全部 Any CPU |

> `Ink Canvas/InkCanvasForClass_*_wpftmp.csproj`、`InkCanvas.Controls/*_wpftmp.csproj` 是 WPF 编译中间产物，**不是真实项目，不要改**。

NativeInk 实验源代码保留但默认构建禁用，不需要 Vortice 包。

### .NET 10 现代化边界

五个现代项目统一使用 .NET 10；SDK 由 `global.json` 选择 10.0 稳定版本。密码派生/恒时比较、随机字节、哈希、NTP socket 与进程信息使用运行时内置 API。Core.Tests 覆盖旧 SHA1 密码迁移、SHA256/TOTP 固定向量、PPT 路径哈希兼容及本地 UDP 的解码/取消行为。不要仅为更新语法改写持久化协议、哈希算法或 Office COM 集成。

**IACoreHelper 是明确保留的 net472/x86 兼容宿主，不属于已迁移的 .NET 10 项目。** 捆绑的 IAWinFX/IALoader/IACore 可以在 .NET 10 x86 中加载并构造 `InkAnalyzer`，但实测在 `Analyze()` 处挂起（形状与文字均复现，STA/MTA/Dispatcher 泵均不能解决）；相同输入在 CLR4/x86 下约一秒返回 Circle。挂起栈位于旧混合模式 `IALoader!<Module>.__crt_dll_initialize()`。不能以“能编译/能加载 DLL”代替完整识别测试，也不能仅修改 TFM 后发布；主程序自包含发布也仍需 .NET Framework 4.7.2+，并随 helper 部署其 `.exe.config`。迁移此宿主必须先解决旧分析引擎的运行时兼容性，并验证现有 IPC、识别上下文提示及 x86/ARM64 发布链路。

## 主项目 MSBuild 目标（改动构建流程前必读）

`Ink Canvas/InkCanvasForClass.csproj` 里有几个自定义 Target，改路径/平台时容易踩：

| Target | 时机 | 作用 |
| --- | --- | --- |
| `CopyIACoreHelper` | AfterTargets=Build，`PublishSingleFile != true` | 复制 IACore helper exe 到主输出目录 |
| `CopyIACoreHelperToPublishDirectory` | AfterTargets=Publish，`PublishSingleFile == true` | 单文件发布时的对应复制 |
| `SetAssemblyInformationalVersion` | AfterTargets=GetBuildVersion | 配合 Nerdbank.GitVersioning 写版本号 |

## 编译前检查

1. 确保没有 CS0246（缺少 using）错误
2. 确保没有 CS0103（找不到名称）错误
3. 确保没有 CS0102（重复定义）错误
4. 确保所有 resx 资源键在默认 resx、en-US、zh-ME 三个文件中完全一致
5. 确保没有未使用的 resx 资源键
6. 新增/修改 UI 文案一律走 i18n，不允许硬编码中文字符串

## 常见编译错误修复

| 错误 | 原因 | 修复方法 |
| --- | --- | --- |
| CS0246 找不到类型 | 缺少 using 指令 | 添加 `using Ink_Canvas.Properties;` 等 |
| CS0103 找不到名称 | 未引用正确命名空间 | 检查是否需要 `using iNKORE.UI.WPF.Modern.Controls;` |
| CS0102 重复定义 | resx Designer.cs 中重复添加属性 | 删除重复的属性声明 |
| XAML 解析错误 | XML 格式错误 | 检查标签闭合、属性引号等 |
| MSB3027 / MSB3021 无法复制，文件被占用 | 上次运行残留的 `InkCanvas.IACoreHelper.exe` 仍在跑，锁住了输出文件 | 先结束残留 helper 进程再重新编译 |
| 找不到 IACoreHelper exe | 辅助项目未生成或 Copy target 路径不匹配 | 检查 `InkCanvas.IACoreHelper/bin/$(Configuration)/` 下的产物及 `CopyIACoreHelper` 目标 |