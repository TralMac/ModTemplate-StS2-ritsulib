Contains 6 templates.

BaseLib templates:

Slay the Spire 2 Mod - Sets up an empty mod with BaseLib as a dependency. If you don't need it, remove it from the csproj and mod manifest json.

Slay the Spire 2 Content - Sets up a content mod.

Slay the Spire 2 Character - Sets up a character mod.

RitsuLib templates (use [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) instead of BaseLib):

| Template | Short name | Contents |
| --- | --- | --- |
| Empty Slay the Spire 2 Mod (RitsuLib) | `ritsusts2mod` | Entry point, RitsuLib patcher with an example patch, empty localization. |
| Slay the Spire 2 Content (RitsuLib) | `ritsusts2contentmod` | Base classes plus an example colorless attack, power card, shared relic and power, with eng/zhs localization. |
| Slay the Spire 2 Character (RitsuLib) | `ritsusts2charmod` | A character with its own card/relic/potion pools, Strike/Defend starter cards, a starter relic, a power, a potion and eng/zhs localization. |

When creating a solution using these templates, make sure to enable "Put solution and project in same directory". This is required for it to work as-is with Godot.

See [wiki](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup) for additional details.
dotnet pack in this folder

## RitsuLib 模板（中文）

```shell
# 在本仓库目录打包并安装模板
dotnet pack
dotnet new install ./bin/Release/Alchyr.Sts2.Templates.<version>.nupkg

# 创建项目（项目名即 Mod id，请使用 PascalCase，例如 MyMod）
dotnet new ritsusts2charmod -n MyMod --ModAuthor YourName
dotnet new ritsusts2contentmod -n MyMod --ModAuthor YourName
dotnet new ritsusts2mod -n MyMod --ModAuthor YourName
```

可选参数：

| 参数 | 说明 |
| --- | --- |
| `--GameApi latest\|0.110\|0.109\|0.107` | 编译所用的游戏 API。`latest` 使用 `STS2.RitsuLib`（跟随 RitsuLib 支持的最高 API，通常是测试版）；其他值使用对应的 `STS2.RitsuLib.Compat.<version>` 包，并同步 `min_game_version`。 |
| `--PublicizeSts true` | 公开化 `sts2.dll` 的非虚私有/受保护成员。 |
| `--NullableChecks disable` | 关闭可空引用检查。 |

模板中的本地化键会按 RitsuLib 的规则（`<MODID>_<类别>_<类名>`，大写下划线形式）自动替换成你的项目名，
例如项目名 `MyMod` 的打击牌键为 `MY_MOD_CARD_STRIKE_MY_MOD.title`。生成项目内的 `README.md` 有更详细的说明。
