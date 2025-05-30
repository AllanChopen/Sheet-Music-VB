# Sheet-Music-VB

A Visual Basic WinForms application for parsing, validating, and generating sheet music from a custom text-based notation. The app exports interactive HTML scores using [VexFlow](https://www.vexflow.com/) for rendering and [Tone.js](https://tonejs.github.io/) for playback.

## Features

- **Custom Notation Parsing:** Write music using simple commands like `nota(c,1);`, `simbolo(silencio,2);`, `simbolo(compas);`, and `tempo(120);`.
- **Syntax Validation:** Checks for correct syntax and ensures each measure (compás) sums to 4 beats.
- **HTML Export:** Generates a standalone HTML file with rendered sheet music and a playback button.
- **Interactive Playback:** Listen to your composition directly in the browser.
- **Easy to Use:** Simple WinForms interface with RichTextBox input and one-click export.

## Example Notation

tempo(120); nota(c,1); nota(d,1); simbolo(silencio,2); simbolo(compas); nota(e,2); nota(f,2); simbolo(compas);

## How to Use

1. **Write your music** in the RichTextBox using the custom notation.
2. **Click the compile button** to validate and export.
3. **Save the generated HTML** when prompted.
4. **Open the HTML file** in your browser to view and play your score.

## Notation Reference

- `tempo(N);` — Set tempo in BPM (e.g., `tempo(120);`)
- `nota(NOMBRE,DURACION);` — Add a note.  
  - `NOMBRE`: c, d, e, f, g, a, b  
  - `DURACION`: 1 (quarter), 2 (half), 4 (whole)
- `simbolo(silencio,DURACION);` — Add a rest.
- `simbolo(compas);` — End of measure (compás).

## Requirements

- Windows with .NET Framework (WinForms)
- Visual Studio 2022 or later

## Output

- HTML file with embedded VexFlow and Tone.js (no server required)
- Interactive score and playback

## License

MIT License

---

*This project is for educational and hobbyist use. Powered by VexFlow and Tone.js.*
