# 小兔（XiaoTu）角色 demo

按 [`docs/骰子角色设计表.xlsx`](../../docs/骰子角色设计表.xlsx) 生成的角色 demo，由 `RitsuCharacterModTemplate` 展开（Mod id `XiaoTu`，RitsuLib）。

## 已实现（设计表的子集）

- 角色「小兔」，起始牌组：打击 ×5、防御 ×5、锈蚀骰子（S1）×1。
- 骰子系统 `XiaoTuCode/Dice/DiceState.cs`：骰子上限 5，普通/临时骰子，总骰点，重投一枚/全部，回合开始重投所有骰子并清除临时骰子。
- 起始遗物「生锈的纪念骰子」：战斗开始获得 1 枚骰子。
- 卡牌：锈蚀骰子、好运打击、好运防御、骰子弹幕、塑料骰子、灌铅骰子。

## 尚未实现

金币为负/债务伤害加成、质押、押大、奇偶判定、其余卡牌与遗物；锈蚀骰子“本场战斗费用+1”；卡面实时显示 x 点；卡牌配图（使用占位图）。

## 注意

云端环境没有 .NET SDK 和 `sts2.dll`，这份代码**没有编译过**。`RustyMemorialDice.BeforeCombatStart` 等钩子名需要在装有游戏的电脑上 `dotnet build` 确认。骰子使用 `System.Random`，不与游戏种子同步。

## 构建与 PCK

本 mod 的 `XiaoTu.json` 声明了 `has_pck: true`，所以必须生成 `XiaoTu.pck`，否则游戏日志会报 “declares that it should load a PCK” 以及一大堆 `res://XiaoTu/images/...` 加载失败。

1. 在本目录新建（已被 .gitignore）`Directory.Build.props`，写入 `GodotPath`（Godot 4.5.1 mono 的 console.exe 路径）。
2. 运行 `dotnet publish`，它会编译并用 Godot 导出 pck 到游戏 `mods/XiaoTu/`。
