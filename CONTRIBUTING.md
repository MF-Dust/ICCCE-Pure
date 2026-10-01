# ICC-CE 代码贡献规范

## 构建环境 / Build environment

- 主程序及 WPF 库使用 .NET 10 SDK，运行需要 Windows 10 19041+ 和 .NET Desktop Runtime 10。
- IACoreHelper 保持 .NET Framework 4.7.2 / x86。PPT 联动仅使用主程序内的 ROT/Office COM，无需加载项构建、注册或签名证书；智慧模式及 WPS 支持需实机回归。
- NativeInk 实验源代码保留但默认构建禁用，无需 Vortice 默认包。
- 构建与保存/自动保存回归检查：
  ```bash
  dotnet build "Ink Canvas.sln" -c Debug -p:Platform=x64
  dotnet run --project InkCanvas.Core.Tests -c Release
  ```
- 可重复的保存性能采样（单页 1000 笔、20 页 × 200 笔；一次预热、五次中位数）：
  ```bash
  dotnet run --project InkCanvas.Core.Tests -c Release -- --save-benchmark
  ```
  分别报告 UI 快照/序列化、后台写入时间、UI 分配与输出字节数；这是当前版本的采样，不是跨版本提速百分比，Office/媒体实机场景另行验证。
- 在交互式 Windows 桌面验证窗口加载、配置重载与正常关闭：
  ```bash
  dotnet run --project InkCanvas.Core.Tests -c Release -- --window-smoke
  ```
  此检查使用临时配置和真实主窗口，跳过应用启动时的文件关联注册，不替代触摸笔和 PowerPoint 实机测试。
- `InkCanvas.NativeInk.Tests` 对应未启用的实验墨迹管线。现有 `PredictionHorizonStaysWithinAdaptiveBounds` 检查在 .NET 6/10 下均失败；修改该管线时需单独运行并检查结果。

## 中文版

### 一、关于人工编写测试代码的要求

1. 进行简单的构建测试，避免提交不完整的代码

2. 进行简单的运行测试，有关UI方面的修改可使用三步法（一看样式，二看行为，三看i18n是否完整），有关后端的修改则遵循“改什么测什么”的规范进行简单测试即可。

### 二、关于使用AI编写代码的要求

1. 进行简单的构建测试，避免提交不完整的代码

2. 进行全面的运行测试，避免可能的回归问题

3. 对于进行了大规模重构，您必须完成以下测试并遵守必要的规定：

   - (1) 对重构的部分进行完整的功能测试，包含所有可能受到影响的局部。
   - (2) 对有变化的部分进行回归问题的可能性检查和测试。
   - (3) 对于测试中发现的问题进行有针对性的修复，这需要仔细核对修改过程中的怀疑导致问题的commit。
   - (4) 严禁盲目使用AI进行修复，当您无法确保自己给出的修改是有效且准确无误时，请严格恪守第(3)点。如果发现您存在不懂装懂的情况且拒不改正我们将您从贡献者中除名，并永久禁止您进行代码贡献。

4. 对于UI的修改遵从人工第二点的同时需要对图标进行必要的检查。

5. 对于后端的修改参照AI第(2)(3)点及人工第2点。

---

## English Version

### I. Requirements for Manually Written Code

1. Perform basic build tests to ensure code compiles successfully and avoid submitting incomplete code.

2. Perform basic runtime tests. For UI-related modifications, use the "Three-Step Approach" (first check styling, second check behavior, third check if i18n is complete). For backend-related modifications, follow the "Test What You Modify" standard to conduct simple testing.

### II. Requirements for AI-Generated Code

1. Perform basic build tests to ensure code compiles successfully and avoid submitting incomplete code.

2. Perform comprehensive runtime tests to actively prevent potential regression issues.

3. For large-scale refactoring, you must complete the following tests and strictly adhere to these regulations:

   - (1) Conduct complete functional testing on the refactored sections, covering all potentially affected components.
   - (2) Inspect and test all changed parts specifically for potential regression issues.
   - (3) Perform targeted fixes for any issues discovered during testing. This requires a meticulous review of individual commits suspected of introducing the bugs.
   - (4) Blindly relying on AI for bug fixing is strictly prohibited. If you cannot guarantee that your proposed fix is effective, valid, and accurate, you must strictly adhere to requirement (3). If any contributor is found pretending to understand AI-generated code they do not fully grasp and refuses to correct this behavior, they will be removed from the project and permanently banned from contributing.

4. For UI modifications, follow rule #2 under Manually Written Code, and perform a thorough check on all modified icons.

5. For backend modifications, refer to rules #(2) and #(3) under AI-Generated Code as well as rule #2 under Manually Written Code.
