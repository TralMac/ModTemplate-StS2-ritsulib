# RitsuContent

由 RitsuLib 内容 Mod 模板生成的《杀戮尖塔 2》Mod：向原版卡池/遗物池添加新内容。

- RitsuLib：https://github.com/BAKAOLC/STS2-RitsuLib
- RitsuLib 文档：https://sts2-ritsulib.ritsukage.com/
- 中文教程：https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/

## 目录结构

```text
RitsuContent.csproj        项目文件（引用 sts2.dll / 0Harmony.dll / STS2.RitsuLib）
RitsuContent.json          Mod 清单，"id" 必须与 MainFile.ModId 一致
Directory.Build.props      本机路径（Godot / 游戏目录），已被 .gitignore 忽略
RitsuContentCode/          C# 代码
  MainFile.cs              初始化入口：注册程序集、创建补丁器
  Cards/ Relics/ Powers/   基类 + 示例内容（无色攻击牌、无色能力牌、共享遗物、能力）
  Patches/                 RitsuLib 补丁系统示例
RitsuContent/              Godot 资源（会被打进 .pck），文件夹名就是 Mod id
  images/                  图片
  localization/eng|zhs/    本地化 json
```

## 构建

1. 安装 .NET 9 SDK 和 MegaDot / Godot 4.5.1 Mono（导出 .pck 用）。
2. 修改 `Directory.Build.props` 中的 `GodotPath`；如果没有自动找到游戏，设置 `Sts2Path`。
3. `dotnet build`：编译 dll，并把 `.dll/.pdb/.json` 复制到 `游戏目录/mods/RitsuContent/`。
   同时 STS2.RitsuLib 包会把对应版本的 RitsuLib 运行时部署到 `mods/STS2-RitsuLib/`（见 csproj 中的 `RitsuLibDeployDir`）。
4. `dotnet publish`：额外用 Godot 导出 `RitsuContent.pck`（图片、本地化等资源）。

发布 Mod 时，玩家需要单独安装完整的 `STS2-RitsuLib` 文件夹（GitHub Release 或创意工坊），不要把 RitsuLib 打包进你的 Mod 文件夹。

## RitsuLib 约定

### 自动注册

`MainFile.Initialize` 中调用了 `ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly)`，之后写在类上的注解会被自动注册：

| 注解 | 作用 |
| --- | --- |
| `[RegisterCard(typeof(ColorlessCardPool))]` | 卡牌加入卡池（也可以是 `IroncladCardPool` 等角色卡池） |
| `[RegisterRelic(typeof(SharedRelicPool))]` | 遗物加入遗物池 |
| `[RegisterPotion(typeof(池))]` | 药水加入药水池 |
| `[RegisterPower]` | 注册能力（`RitsuContentPower` 基类已带 `Inherit = true`，子类自动注册） |

也可以在初始化时用 `RitsuLibFramework.CreateContentPack(ModId)...Apply()` 集中注册，同一个内容只用一种方式注册。

### ID 与本地化键

RitsuLib 注册的内容会得到固定的公开 Entry：`<MODID>_<类别>_<类名>`，三段都会转换成大写下划线形式。
例如 Mod id 为 `RitsuContent` 时：

| 类 | Entry（本地化键前缀） | 本地化文件 |
| --- | --- | --- |
| `ExampleAttack` | `RITSU_CONTENT_CARD_EXAMPLE_ATTACK` | `cards.json` |
| `ExampleRelic` | `RITSU_CONTENT_RELIC_EXAMPLE_RELIC` | `relics.json` |
| `ExamplePower` | `RITSU_CONTENT_POWER_EXAMPLE_POWER` | `powers.json` |

Entry 会写进存档，发布后不要再改类名；确实要改名时用 `[RegisterCard(typeof(池), StableEntryStem = "旧名")]` 保持 Entry 不变。
类名请使用 `PascalCase`，避免 `TESTCARD`、`URLParser` 这种全大写写法。

游戏内控制台（`~`）可以用 `card RITSU_CONTENT_CARD_EXAMPLE_ATTACK`、`relic ...`、`power ... 1 0` 测试内容。

### 图片

基类按类名查找图片，找不到时使用占位图：

| 内容 | 路径 |
| --- | --- |
| 卡牌 | `RitsuContent/images/cards/<类名>.png`（250x190，找不到时使用 RitsuLib 内置卡图） |
| 遗物 | `RitsuContent/images/relics/<类名>.png`、`<类名>_outline.png`、`big/<类名>.png` |
| 能力 | `RitsuContent/images/powers/<类名>.png`、`big/<类名>.png` |

新加卡牌后可以运行 `python tools/card_placeholders.py`（需要 `pip install pillow`）批量生成占位卡图：按卡牌类型配色的大色块加卡名（默认取 `zhs` 本地化，`--lang eng` 用英文名）。只生成还没有图片的卡，`--force` 会覆盖全部。

### 游戏版本

csproj 引用的 `STS2.RitsuLib` 跟随 RitsuLib 支持的最高游戏 API（通常是测试版）。
如果你的 Mod 面向正式版等较旧分支，改用对应的 `STS2.RitsuLib.Compat.<api-version>` 包（创建项目时可用 `--GameApi` 选择）。
