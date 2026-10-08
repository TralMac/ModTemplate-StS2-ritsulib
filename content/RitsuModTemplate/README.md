# RitsuMod

由 RitsuLib 空白 Mod 模板生成的《杀戮尖塔 2》Mod。

- RitsuLib：https://github.com/BAKAOLC/STS2-RitsuLib
- RitsuLib 文档：https://sts2-ritsulib.ritsukage.com/
- 中文教程：https://glitchedreme.github.io/SlayTheSpire2ModdingTutorials/

## 目录结构

```text
RitsuMod.csproj            项目文件（引用 sts2.dll / 0Harmony.dll / STS2.RitsuLib）
RitsuMod.json              Mod 清单，"id" 必须与 MainFile.ModId 一致
Directory.Build.props      本机路径（Godot / 游戏目录），已被 .gitignore 忽略
RitsuModCode/              C# 代码
  MainFile.cs              初始化入口：注册程序集、创建补丁器
  Patches/                 RitsuLib 补丁系统示例
RitsuMod/                  Godot 资源（会被打进 .pck），文件夹名就是 Mod id
  localization/eng|zhs/    本地化 json
```

## 构建

1. 安装 .NET 9 SDK 和 MegaDot / Godot 4.5.1 Mono（导出 .pck 用）。
2. 修改 `Directory.Build.props` 中的 `GodotPath`；如果没有自动找到游戏，设置 `Sts2Path`。
3. `dotnet build`：编译 dll，并把 `.dll/.pdb/.json` 复制到 `游戏目录/mods/RitsuMod/`。
   同时 STS2.RitsuLib 包会把对应版本的 RitsuLib 运行时部署到 `mods/STS2-RitsuLib/`（见 csproj 中的 `RitsuLibDeployDir`）。
4. `dotnet publish`：额外用 Godot 导出 `RitsuMod.pck`（图片、本地化等资源）。

发布 Mod 时，玩家需要单独安装完整的 `STS2-RitsuLib` 文件夹（GitHub Release 或创意工坊），不要把 RitsuLib 打包进你的 Mod 文件夹。

## 常用入口

| 需求 | 使用 |
| --- | --- |
| 注册卡牌、遗物、药水、能力、角色 | 类上的 `[RegisterCard(typeof(池))]` 等注解，或 `RitsuLibFramework.CreateContentPack(ModId)` |
| Patch 游戏方法 | `RitsuLibFramework.CreatePatcher(ModId, name)` + `IPatchMethod`（见 `Patches/`） |
| 响应框架或游戏时机 | `RitsuLibFramework.SubscribeLifecycle<TEvent>(...)` |
| 存储档案或账号数据 | `RitsuLibFramework.BeginModDataRegistration(ModId)` 与 `GetDataStore(ModId)` |
| 存储跑局内数据 | `RitsuLibFramework.GetRunSavedDataStore(ModId)` |
| 添加玩家可编辑设置页 | `RitsuLibFramework.RegisterModSettings(ModId, configure)` |

需要卡牌、遗物等示例时，可以改用 `ritsusts2contentmod`（内容 Mod）或 `ritsusts2charmod`（角色 Mod）模板。

### ID 与本地化键

RitsuLib 注册的内容会得到固定的公开 Entry：`<MODID>_<类别>_<类名>`，三段都会转换成大写下划线形式。
例如 Mod id 为 `RitsuMod` 时，卡牌 `MyCard` 的 Entry 是 `RITSU_MOD_CARD_MY_CARD`，本地化写在 `RitsuMod/localization/<语言>/cards.json`：

```json
{
  "RITSU_MOD_CARD_MY_CARD.title": "My Card",
  "RITSU_MOD_CARD_MY_CARD.description": "Deal {Damage:diff()} damage."
}
```

### 游戏版本

csproj 引用的 `STS2.RitsuLib` 跟随 RitsuLib 支持的最高游戏 API（通常是测试版）。
如果你的 Mod 面向正式版等较旧分支，改用对应的 `STS2.RitsuLib.Compat.<api-version>` 包（创建项目时可用 `--GameApi` 选择）。
