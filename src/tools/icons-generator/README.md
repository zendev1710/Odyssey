# Odyssey.Icons.Generator

Development tool that generates the localization data used by the icon resolver.
It writes deterministic JSON under `src/app/Odyssey.Desktop/Assets/i18n/`.

The server stays German: German names live only in the generated data, never in
the application code.

## Usage

```powershell
# Spells, read from an Eressea server checkout
dotnet run --project src/tools/icons-generator -- --server C:\path\to\eressea\server

# Icons, read from the checked-in icon table
dotnet run --project src/tools/icons-generator -- --icons

# Verify that the generated catalogs are current
dotnet run --project src/tools/icons-generator -- --icons-check
```

Spell output defaults to `src/app/Odyssey.Desktop/Assets/i18n/spells.json`.
Icon output defaults to `src/app/Odyssey.Desktop/Assets/i18n/icons.json`.
Paths are resolved relative to the repository root.

| Option | Description |
|---|---|
| `--server <path>` | Eressea server directory. Can also be set with the `ERESSEA_SERVER` environment variable. |
| `--out <file>` | Spell output file. |
| `--code-only` | Only include spells registered in `spells.c`. |
| `--check` | Compare the existing spell catalog with the generated one; exit code 1 when stale. |
| `--icons` | Generate the icon catalog from `data/icons.tsv`. |
| `--icons-out <file>` | Icon output file. |
| `--icons-check` | Compare the existing icon catalog with the generated one; exit code 1 when stale. |
| `--manifest <file>` | Icon table to read (default: `data/icons.tsv`). |
| `--assets <dir>` | Asset root used to validate icon files. |
| `-h`, `--help` | Show help. |

## Spell catalog

`spells.json` maps the spell identifier emitted by the server in `ZAUBER` blocks
to its English and German names:

```json
{
  "version": 1,
  "spells": [
    { "id": 992591507, "sname": "treegrow", "en": "Grove of Oak Trees", "de": "Hain des Lebens" }
  ]
}
```

The identifier is `str_hash(sname)` as implemented in
`eressea/server/clibs/strings.c`:

```c
int key = 0;
while (*s) key = key * 37 + *s++;
return key & 0x7FFFFFFF;
```

The catalog is the union of:

- the spells registered in `spell_functions[]` in `src/spells.c`;
- the spell names translated in `res/translations/strings.{de,en}.po`.

The union keeps older reports readable and covers data-only spells created from
the server configuration, which never appear in `spell_functions[]`. Entries
without a translation fall back to the server identifier. Spell identifiers must
not collide: the generator fails if two names hash to the same value.

## Icon catalog

`icons.json` maps the German report vocabulary and English names to the English
asset tree under `Assets/`:

```json
{
  "category": "building",
  "id": "building/mine",
  "role": "entry",
  "de": "Bergwerk",
  "en": "Mine",
  "aliases": ["bergwerk", "mines"],
  "file": "buildings/mine.gif"
}
```

`data/icons.tsv` is the source of truth and is never written by the generator.
It has one row per asset:

| Column | Meaning |
|---|---|
| `category` | Resolver category (`animal`, `building`, `item`, `terrain`, ...). |
| `id` | Stable `category/slug` identifier. |
| `role` | `entry` (normal icon), `category` (category icon), or `variant` (preserved alternate art). |
| `de` | German report name, or empty for variants. |
| `en` | English name, or empty for variants. |
| `aliases` | Additional lookup names separated by `|`. |
| `file` | Path below `Assets/`, for example `items/sword.gif`. |

The generator validates that ids and files are unique, paths are lowercase ASCII,
and no two primary icons in the same category claim the same normalized alias. It
also adds English singular/plural aliases when they do not conflict with an
explicit entry.

Variants keep alternate artwork in the repository but never participate in name
lookup. The runtime resolver (`Odyssey.Icons.IconCatalog`) ignores them.
