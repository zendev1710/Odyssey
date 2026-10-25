# Odyssey.Icons.Generator

Development tool that generates the localization data used by the icon resolver.
It reads the Eressea server sources and writes deterministic JSON under
`src/app/Odyssey.Desktop/Assets/i18n/`.

The server stays German: German names live only in the generated data, never in
the application code.

## Usage

```powershell
dotnet run --project src/tools/icons-generator -- --server C:\path\to\eressea\server
```

The output defaults to
`src/app/Odyssey.Desktop/Assets/i18n/spells.json` (located relative to the
repository root). Override it with `--out`.

| Option | Description |
|---|---|
| `--server <path>` | Eressea server directory. Can also be set with the `ERESSEA_SERVER` environment variable. |
| `--out <file>` | Output file. |
| `--code-only` | Only include spells registered in `spells.c`. |
| `--check` | Compare the existing file with the generated one; exit code 1 when stale. |
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

## Tests

```powershell
dotnet test src/tests/Odyssey.Icons.Generator.Tests
```

The tests pin the hash against the server unit test (`Hodor`) and against values
observed in real reports (`treegrow`, `irongolem`).
