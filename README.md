# BlindBoxPlugin

用来统计盲盒中物品获得情况的插件。支持特殊配给货箱、庆典礼物箱、上锁的宝箱、九宫幻卡包、能源包、宇宙好运道信用点等多种容器，一键查看已获得与未获得的物品。

![image1.png](./images/image1.png)

## 功能

- 查看各类盲盒中包含的所有物品及其获得状态
- 物品按颜色区分状态：
  - 🟢 绿色 — 已获得
  - ⬜ 白色 — 未获得，可交易
  - ⬛ 灰色 — 未获得，不可交易
- 带 `*` 标记的物品为仅可从当前途径获取
- 支持按「所有 / 已获得 / 未获得」筛选显示
- 点击物品名称可复制到剪切板并发送物品链接到聊天框
- 设置界面提供物品名称 → 物品 ID 查询工具

## 支持的盲盒类型

| 分类 | 名称 |
|------|------|
| 特殊配给货箱 | 特殊配给货箱（重生/苍穹）、特殊配给货箱（红莲） |
| 庆典礼物箱 | 庆典礼物箱 |
| 无人岛 | 无人岛特殊配给货箱 |
| 上锁的宝箱 | 常风地带、恒冰地带、涌火地带、丰水地带、南方战线、高原 |
| 九宫幻卡包 | 白金包、铜包、银包、金包、灵银包、帝国包、梦想包 |
| 能源包 | 俄匊斯能源包、奥克塞西亚能源包 |
| 宇宙好运道 | 月球信用点、法恩娜信用点、俄匊斯信用点、奥克塞西亚信用点 |

## 使用方法

| 命令 | 说明 |
|------|------|
| `/blindbox` | 打开盲盒信息界面 |
| `/blindbox config` | 打开设置界面 |

也可通过卫月插件列表中的「打开主界面」/「打开设置」按钮访问。

## How to get

1. 卫月设置 → 测试版
2. 添加仓库 `https://raw.githubusercontent.com/yimo0908/FFXIVBlindBoxPlugin/main/repo.json` 并启用
3. 从插件列表中安装

## 项目结构

```
FFXIVBlindBoxPlugin/
├── BlindBoxPlugin/                 # 插件主项目
│   ├── BlindBoxPlugin.csproj       # 项目文件，SDK 为 Dalamud.CN.NET.Sdk
│   ├── BlindBoxPlugin.json         # 插件元数据（名称、描述、图标等）
│   ├── Plugin.cs                   # 插件入口，注册命令与窗口
│   ├── Configuration.cs            # 插件配置（选中的盲盒 ID、显示模式）
│   ├── Data.cs                     # 盲盒数据定义与查找表 BlindBoxInfoMap
│   ├── Models.cs                   # DisplayMode 枚举
│   ├── GameFunctions.cs            # 物品解锁状态检测
│   └── Windows/
│       ├── MainWindows.cs          # 主窗口：布局、盲盒选择列表、物品表格
│       ├── MainWindows.Draw.cs     # 主窗口：图标绘制、颜色状态、* 标记
│       └── MainWindows.Game.cs     # 主窗口：聊天链接、剪切板复制、游戏内提示
├── images/                         # 截图与图标
├── BlindBoxPlugin.sln              # 解决方案文件
├── LICENSE                         # MIT 许可证
└── README.md
```

### 核心文件说明

| 文件 | 职责 |
|------|------|
| `Plugin.cs` | 插件入口点，注册 `/blindbox` 命令，初始化 `MainWindow` 与 `ConfigWindow` |
| `Data.cs` | 所有盲盒数据的定义。每个盲盒为一个 `BlindBoxInfo` 实例，最终注册到 `BlindBoxInfoMap` 查找表 |
| `Models.cs` | `DisplayMode` 枚举（所有/已获得/未获得）控制显示筛选 |
| `GameFunctions.cs` | 通过 FFXIVClientStructs 的 `UIState->IsItemActionUnlocked` 判断物品是否已解锁 |
| `Configuration.cs` | 持久化配置：当前选中的盲盒 ID 和显示模式 |
| `MainWindows.*.cs` | 主窗口使用 partial class 拆分为三个文件，分别负责布局、绘制和游戏交互 |

## 维护说明

### 本地构建

1. 安装 [.NET 9 SDK](https://dotnet.microsoft.com/download) 或更高版本
2. 下载最新的 Dalamud CN 构建并解压到本地目录（如 `BlindBoxPlugin/Dalamud`）
3. 设置环境变量后构建：

   ```powershell
   $env:DALAMUD_HOME = "路径\到\Dalamud"
   dotnet build BlindBoxPlugin -c Release
   ```

### 新增或更新盲盒数据

盲盒数据全部硬编码在 `Data.cs` 中，不依赖外部数据文件。新增一个盲盒需要：

1. 在 `BlindBoxData` 类中创建 `BlindBoxInfo` 实例：
   ```csharp
   private static readonly BlindBoxInfo NewBox = new(
       盲盒道具ID,
       [物品ID列表],
       [仅可从此途径获取的物品ID列表]  // 可选，没有则省略
   );
   ```
2. 将实例注册到 `BlindBoxInfoMap`：
   ```csharp
   [盲盒道具ID] = NewBox,
   ```

**关于 `UniqueItems`**：标记为 unique 的物品在界面中会显示 `*` 前缀，表示该物品仅可从当前盲盒途径获取。如果某物品可通过多种途径获得，则不应加入 unique 列表。

**获取物品 ID**：可使用设置界面（`/blindbox config` →「获取物品Id」标签页）输入物品名称批量查询 ID，也可通过 [Garland Tools](https://garlandtools.org/db/) 或游戏解包数据查找。

### 解锁检测原理

`GameFunctions.IsUnlocked(Item item)` 通过以下方式判断物品是否已获得：

1. 调用 `ExdModule.GetItemRowById` 获取物品的 EXD 行数据
2. 调用 `UIState.Instance()->IsItemActionUnlocked(row)` 检查物品动作是否已解锁
3. 返回 `1` 表示已解锁（已获得），其他值表示未获得

该方法依赖 FFXIVClientStructs 的原生函数，游戏大版本更新后可能需要更新 ClientStructs 绑定。

### ActionType 参考

以下列出 `GameFunctions.IsUnlocked` 涉及的物品动作类型（`ActionType`）及其 RowId，供维护时参考。该枚举已从源码中移除，仅保留为文档：

| ActionType | RowId | 说明 |
|------------|-------|------|
| Companion | 853 | 宠物 |
| BuddyEquip | 1013 | 鸟甲 |
| Mount | 1322 | 坐骑 |
| SecretRecipeBook | 2136 | 秘籍 |
| UnlockLink | 2633 | 解锁链接（骑乘图、青魔法图腾、情感动作/舞蹈、发型等） |
| TripleTriadCard | 3357 | 九宫幻卡 |
| FolkloreTome | 4107 | 传承录 |
| OrchestrionRoll | 25183 | 管弦乐琴乐谱 |
| FramersKit | 29459 | 画框套装 |
| Ornament | 20086 | 时装配件 |
| Glasses | 37312 | 眼镜 |
| CompanySealVouchers | 41120 | 军团券（use = 在军团中，is unlocked = 始终 false） |

### 发布流程

项目使用 GitHub Actions 自动化发布（见 `.github/workflows/main.yml`）：

1. **构建**：每次 push 到 `main` 或 PR 时自动构建验证
2. **发布**：当创建以 `v` 开头的 Release tag 时触发：
   - 构建 Release 产物并打包为 `latest.zip`
   - 上传到 GitHub Release
   - 运行 `Make-Repo.ps1` 生成 `repo.json`（插件仓库清单文件）
   - 自动提交 `repo.json` 到 `main` 分支

> **注意**：`Make-Repo.ps1` 中的下载链接模板目前硬编码为 `he0119` 仓库地址。如果是 fork 仓库自行发布，需要修改该脚本中的 `$dlTemplate` 为自己的仓库地址。

## Credits

- 查询物品获取情况的函数来自 [CriticalCommonLib](https://github.com/Critical-Impact/CriticalCommonLib)
- 盲盒数据来自 [最终幻想XIV中文维基](https://ff14.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5)

## License

[MIT](./LICENSE)
