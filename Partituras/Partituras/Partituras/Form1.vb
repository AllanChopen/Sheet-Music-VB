Imports System.IO
Imports System.Text

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim textoCancion As String = RichTextBox1.Text


        Try
            MyParser.Setup()
            Dim resultado As Boolean = MyParser.Parse(New StringReader(textoCancion))
            If Not resultado Then
                MessageBox.Show("Error en la compilación. Verifica la sintaxis.", "Compilador", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
        Catch ex As Exception
            MessageBox.Show("Ocurrió un error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try


        ' Validar compases
        Dim lineas = NormalizarExpresiones(textoCancion)
        Dim sumaCompas As Double = 0
        Dim compasInvalido As Boolean = False
        Dim compasNum As Integer = 1

        For Each lineaOriginal In lineas
            Dim linea = lineaOriginal.Trim().ToLowerInvariant()

            If linea.StartsWith("nota(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(5, linea.Length - 7)
                Dim partes = contenido.Split(","c)
                If partes.Length = 2 Then
                    Dim duracionStr = partes(1).Trim()
                    If IsNumeric(duracionStr) Then
                        sumaCompas += ValorDuracion(Integer.Parse(duracionStr))
                    End If
                End If
            ElseIf linea.StartsWith("simbolo(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(8, linea.Length - 10).Trim()
                Dim partesContenido = contenido.Split(","c)
                If partesContenido.Length = 2 AndAlso partesContenido(0).Trim() = "silencio" Then
                    Dim duracionStr = partesContenido(1).Trim()
                    If IsNumeric(duracionStr) Then
                        sumaCompas += ValorDuracion(Integer.Parse(duracionStr))
                    End If
                ElseIf contenido = "compas" Then
                    If Math.Abs(sumaCompas - 4.0) > 0.001 Then
                        compasInvalido = True
                        Exit For
                    End If
                    sumaCompas = 0
                    compasNum += 1
                End If

            ElseIf linea.StartsWith("simbolo(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(8, linea.Length - 10).Trim()
                If contenido = "compas" Then
                    If Math.Abs(sumaCompas - 4.0) > 0.001 Then
                        compasInvalido = True
                        Exit For
                    End If
                    sumaCompas = 0
                    compasNum += 1
                End If
            End If
        Next

        ' Validar el último compás si no termina con compas
        If Not compasInvalido AndAlso sumaCompas > 0 Then
            If Math.Abs(sumaCompas - 4.0) > 0.001 Then
                compasInvalido = True
            End If
        End If

        If compasInvalido Then
            MessageBox.Show("Error: Hay un compás cuya suma de notas y silencios no es igual a 4. Por favor, corrige los datos e inténtalo de nuevo.", "Error de compás", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        MessageBox.Show("Compilación exitosa.", "Compilador", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Using saveDialog As New SaveFileDialog()
            saveDialog.Title = "Guardar partitura como HTML"
            saveDialog.Filter = "Archivos HTML (*.html)|*.html"
            saveDialog.InitialDirectory = "C:\Users\allan\Documents\GoldParser\Partituras\PartiturasHTML"
            saveDialog.FileName = "Partitura.html"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                Dim rutaHtml As String = saveDialog.FileName
                Dim titulo As String = Path.GetFileNameWithoutExtension(rutaHtml)
                Dim html As String = GenerarHtmlPartitura(textoCancion, titulo)
                File.WriteAllText(rutaHtml, html, Encoding.UTF8)
                MessageBox.Show("HTML generado correctamente en: " & rutaHtml, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Process.Start(New ProcessStartInfo(rutaHtml) With {.UseShellExecute = True})
            End If
        End Using
    End Sub



    Public Function GenerarHtmlPartitura(codigo As String, titulo As String) As String
        Dim lineas = NormalizarExpresiones(codigo)
        Dim compasActual As New List(Of String)
        Dim compasesSeparados As New List(Of List(Of String))()
        Dim tempoValor As Integer = 120 ' Valor por defecto

        For Each lineaOriginal In lineas
            Dim linea = lineaOriginal.Trim().ToLowerInvariant()

            ' Tempo
            If linea.StartsWith("tempo(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(6, linea.Length - 8).Trim()
                If IsNumeric(contenido) Then tempoValor = Integer.Parse(contenido)
                Continue For
            End If

            ' Nota
            If linea.StartsWith("nota(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(5, linea.Length - 7)
                Dim partes = contenido.Split(","c)
                If partes.Length = 2 Then
                    Dim nota = partes(0).Trim()
                    Dim duracionStr = partes(1).Trim()
                    If IsNumeric(duracionStr) Then
                        Dim duracionNum = Integer.Parse(duracionStr)
                        Dim duracionVF As String = ""
                        Select Case duracionNum
                            Case 1 : duracionVF = "q"
                            Case 2 : duracionVF = "h"
                            Case 4 : duracionVF = "w"
                            Case Else : Continue For
                        End Select
                        Dim notasValidas = {"c", "d", "e", "f", "g", "a", "b"}
                        If notasValidas.Contains(nota) Then
                            Dim vexNota = $"{nota}/4"
                            compasActual.Add($"new VF.StaveNote({{ keys: ['{vexNota}'], duration: '{duracionVF}' }})")
                        End If
                    End If
                End If
                Continue For
            End If

            ' Silencio o compás
            If linea.StartsWith("simbolo(") AndAlso linea.EndsWith(");") Then
                Dim contenido = linea.Substring(8, linea.Length - 10).Trim()
                Dim partesContenido = contenido.Split(","c)
                If partesContenido.Length = 2 AndAlso partesContenido(0).Trim() = "silencio" Then
                    Dim duracionStr = partesContenido(1).Trim()
                    If IsNumeric(duracionStr) Then
                        Dim duracionNum = Integer.Parse(duracionStr)
                        Dim duracionVF As String = ""
                        Select Case duracionNum
                            Case 1 : duracionVF = "qr"
                            Case 2 : duracionVF = "hr"
                            Case 4 : duracionVF = "wr"
                            Case Else : Continue For
                        End Select
                        compasActual.Add($"new VF.StaveNote({{ keys: ['b/4'], duration: '{duracionVF}' }})")
                    End If
                ElseIf contenido = "compas" Then
                    If compasActual.Count > 0 Then
                        compasesSeparados.Add(New List(Of String)(compasActual))
                        compasActual.Clear()
                    End If
                End If
            End If
        Next

        If compasActual.Count > 0 Then
            compasesSeparados.Add(compasActual)
        End If



        ' --- Generar HTML ---
        Dim html As New StringBuilder()
        html.AppendLine("<!DOCTYPE html>")
        html.AppendLine("<html lang='es'>")
        html.AppendLine("<head>")
        html.AppendLine("  <meta charset='UTF-8'>")
        html.AppendLine($"  <title>{titulo}</title>")
        html.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/vexflow/releases/vexflow-min.js'></script>")
        html.AppendLine("  <script src='https://cdn.jsdelivr.net/npm/tone@14.7.77/build/Tone.min.js'></script>")
        html.AppendLine("  <style>body { font-family: Arial; } #partitura { padding: 20px; }</style>")
        html.AppendLine("</head>")
        html.AppendLine("<body>")
        html.AppendLine($"<h2>{titulo}</h2>")
        html.AppendLine("<div id='partitura'></div>")

        ' VexFlow Script
        html.AppendLine("<script>")
        html.AppendLine("const VF = Vex.Flow;")
        html.AppendLine("const div = document.getElementById('partitura');")
        Dim totalCompases As Integer = compasesSeparados.Count
        html.AppendLine("const compasesPorLinea = 2;")
        html.AppendLine($"const totalCompases = {totalCompases};")
        html.AppendLine("const anchoCompas = 300;")
        html.AppendLine("const altoStave = 150;")
        html.AppendLine("const lineas = Math.ceil(totalCompases / compasesPorLinea);")
        html.AppendLine("div.innerHTML = '';")
        html.AppendLine("const notesPorCompas = [];")
        For Each compas In compasesSeparados
            html.AppendLine("notesPorCompas.push([")
            html.AppendLine("  " & String.Join("," & vbCrLf & "  ", compas))
            html.AppendLine("]);")
        Next
        html.AppendLine("const renderers = [], contexts = [], staves = [];")
        html.AppendLine("for(let i = 0; i < lineas; i++) {")
        html.AppendLine("  const svgContainer = document.createElement('div');")
        html.AppendLine("  div.appendChild(svgContainer);")
        html.AppendLine("  const renderer = new VF.Renderer(svgContainer, VF.Renderer.Backends.SVG);")
        html.AppendLine("  renderer.resize(anchoCompas * compasesPorLinea, altoStave);")
        html.AppendLine("  renderers.push(renderer);")
        html.AppendLine("  const context = renderer.getContext();")
        html.AppendLine("  contexts.push(context);")
        html.AppendLine("  const stave = new VF.Stave(10, 40, anchoCompas * compasesPorLinea - 20);")
        html.AppendLine("  stave.addClef('treble').addTimeSignature('4/4');")
        html.AppendLine("  stave.setContext(context).draw();")
        html.AppendLine("  staves.push(stave);")
        html.AppendLine("}")
        html.AppendLine("for(let i = 0; i < lineas; i++) {")
        html.AppendLine("  const notas = [];")
        html.AppendLine("  const startCompas = i * compasesPorLinea;")
        html.AppendLine("  const endCompas = Math.min(startCompas + compasesPorLinea, totalCompases);")
        html.AppendLine("  for(let j = startCompas; j < endCompas; j++) {")
        html.AppendLine("    notas.push(...notesPorCompas[j]);")
        html.AppendLine("    notas.push(new VF.BarNote());")
        html.AppendLine("  }")
        html.AppendLine("  const voice = new VF.Voice({ num_beats: 4 * (endCompas - startCompas), beat_value: 4 });")
        html.AppendLine("  voice.addTickables(notas);")
        html.AppendLine("  new VF.Formatter().joinVoices([voice]).format([voice], anchoCompas * compasesPorLinea - 40);")
        html.AppendLine("  voice.draw(contexts[i], staves[i]);")
        html.AppendLine("}")
        html.AppendLine($"const tempoValor = {tempoValor};")
        html.AppendLine("contexts[0].setFont('Arial', 14, '').setFillStyle('#000');")
        html.AppendLine("contexts[0].fillText('Tempo: ' + tempoValor + ' bpm', 10, 20);")
        html.AppendLine("</script>")

        ' Botón reproducir y Tone.js
        html.AppendLine("<button onclick='reproducir()' style='margin: 20px; padding: 10px;'>▶ Reproducir</button>")
        html.AppendLine("<script>")
        html.AppendLine("async function reproducir() {")
        html.AppendLine("  await Tone.start();")
        html.AppendLine("  const synth = new Tone.Synth({ envelope: { attack: 0.01, decay: 0.1, sustain: 0.3, release: 0.3 } }).toDestination();")
        html.AppendLine("  let now = Tone.now();")
        html.AppendLine($"  const bpm = {tempoValor};")
        html.AppendLine("  const negraDuracion = 60 / bpm;")
        html.AppendLine("  const notas = [")
        For Each compas In compasesSeparados
            For Each nota In compas
                Dim notaMusical As String = "C4"
                Dim startIndex = nota.IndexOf("keys: [")
                If startIndex >= 0 Then
                    Dim keyPart = nota.Substring(startIndex)
                    Dim match = System.Text.RegularExpressions.Regex.Match(keyPart, "'([a-g]/[0-9])'")
                    If match.Success Then
                        notaMusical = match.Groups(1).Value.ToUpper().Replace("/", "")
                    End If
                End If
                If nota.Contains("duration: 'q'") Then
                    html.AppendLine($"    {{ nota: '{notaMusical}', dur: negraDuracion }},")
                ElseIf nota.Contains("duration: 'h'") Then
                    html.AppendLine($"    {{ nota: '{notaMusical}', dur: negraDuracion * 2 }},")
                ElseIf nota.Contains("duration: 'w'") Then
                    html.AppendLine($"    {{ nota: '{notaMusical}', dur: negraDuracion * 4 }},")
                ElseIf nota.Contains("duration: 'qr'") Then
                    html.AppendLine("    { nota: null, dur: negraDuracion },")
                ElseIf nota.Contains("duration: 'hr'") Then
                    html.AppendLine("    { nota: null, dur: negraDuracion * 2 },")
                ElseIf nota.Contains("duration: 'wr'") Then
                    html.AppendLine("    { nota: null, dur: negraDuracion * 4 },")
                End If
            Next
        Next
        html.AppendLine("  ];")
        html.AppendLine("  for (let i = 0; i < notas.length; i++) {")
        html.AppendLine("    const n = notas[i];")
        html.AppendLine("    if (n.nota) synth.triggerAttackRelease(n.nota, n.dur, now);")
        html.AppendLine("    now += n.dur + 0.05;")
        html.AppendLine("  }")
        html.AppendLine("}")
        html.AppendLine("</script>")
        html.AppendLine("</body>")
        html.AppendLine("</html>")

        Return html.ToString()
    End Function


    Private Function ValorDuracion(duracion As Integer) As Double
        ' 1 = negra (1), 2 = blanca (2), 4 = redonda (4)
        Select Case duracion
            Case 1 : Return 1.0 ' negra
            Case 2 : Return 2.0 ' blanca
            Case 4 : Return 4.0 ' redonda
            Case Else : Return 0.0
        End Select
    End Function


    Private Function NormalizarExpresiones(texto As String) As List(Of String)
        ' Reemplaza saltos de línea y tabulaciones por espacio
        Dim limpio = texto.Replace(vbCr, " ").Replace(vbLf, " ").Replace(vbTab, " ")
        ' Reemplaza múltiples espacios por uno solo
        Do While limpio.Contains("  ")
            limpio = limpio.Replace("  ", " ")
        Loop
        ' Divide por punto y coma y limpia cada expresión
        Dim expresiones = limpio.Split(";"c, StringSplitOptions.RemoveEmptyEntries)
        Dim resultado As New List(Of String)
        For Each expr In expresiones
            Dim e = expr.Trim()
            If e.Length > 0 Then
                resultado.Add(e & ";") ' Agrega el punto y coma de nuevo para que coincida con tu lógica
            End If
        Next
        Return resultado
    End Function


End Class



