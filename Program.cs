using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WPF_Hide_Names
{
    internal class Program
    {
        static Dictionary<string, string> globalRenameRules = new Dictionary<string, string>();
        static HashSet<string> usedHexNames = new HashSet<string>();

        // Расширенный черный список, чтобы обфускатор не трогал важные для WPF и WMI имена
        static HashSet<string> blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Базовые ключевые слова и системные объекты
            "Class", "Name", "Command", "Binding", "Path", "String", "Main", "App", "Application",
            "System", "Windows", "Controls", "Data", "Visibility", "True", "False", "Null", "true", "false",
            "obj", "sender", "e", "DataContext", "Content", "Tag", "Instance", "Item", "Count", "Length",
            
            // Критические свойства WPF и интерфейсов (CanExecuteChanged теперь под полной защитой)
            "Type", "IsEnabled", "Duration", "Opacity", "Color", "Margin", "Padding", "Width", "Height",
            "Source", "Target", "Property", "Storyboard", "Setter", "Trigger", "Grid", "StackPanel", "Border",
            "TextBlock", "Button", "ContentControl", "UserControl", "Window", "Canvas", "Image", "Rectangle",
            "HorizontalAlignment", "VerticalAlignment", "Foreground", "Background", "FontSize", "FontWeight",
            "FontFamily", "FontStyle", "Mode", "OneWay", "TwoWay", "OneTime", "OneWayToSource", "UpdateSourceTrigger",
            "TargetProperty", "To", "From", "By", "Key", "X", "Y", "TargetName", "Converter", "ConverterParameter",
            "TargetType", "Resources", "Styles", "Style", "Value", "Format", "Text", "IsExpanded", "IsSelected",
            "IsChecked", "IsOpen", "ItemsSource", "SelectedIndex", "SelectedItem", "SelectedValue", "Orientation",
            "VerticalScrollBarVisibility", "HorizontalScrollBarVisibility", "ClipToBounds", "IsHitTestVisible",
            "Focusable", "Cursor", "CornerRadius", "SnapsToDevicePixels", "UseLayoutRounding", "StaticResource", "DynamicResource",
            "CanExecuteChanged", "CanExecute", "Execute", "Show", "GetValue", "SetValue", "RaiseEvent", "AddHandler", "RemoveHandler",
            "OnPropertyChanged", "GetString","OnValueChanged"
        };

        static HashSet<string> methodBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Main", "InitializeComponent", "OnStartup", "OnExit", "CanExecute", "Execute",
            "Convert", "ConvertBack", "Initialize", "OnPropertyChanged", "RefreshUI", "OnClosed", "Close",
            "ToString", "Equals", "GetHashCode", "GetType", "Invoke", "CanExecuteChanged","CanExecuteChanged", "CanExecute",
            "Execute", "Show", "GetValue", "SetValue", "RaiseEvent", "AddHandler", "RemoveHandler", "OnPropertyChanged", "Refresh", "GetString","OnValueChanged",
            "GetFormat", "SetFormat", "GetText", "SetText", "GetAccent", "SetAccent", "Shutdown", "Start", "Stop", "Clear", "Add", "Remove", "Update",

        };

        class AdvancedDeadCodeGenerator
        {
            private static readonly Random _rnd = new Random();

            public static string GenerateAdvancedDeadCode()
            {
                // Случайно выбираем один из 4 стилей генерации кода
                int style = _rnd.Next(1, 5);

                switch (style)
                {
                    case 1: return GenerateNetworkFallbackStyle();
                    case 2: return GenerateCryptoValidationStyle();
                    case 3: return GenerateConfigParsingStyle();
                    case 4: return GenerateHardwareMetricsStyle();
                    default: return string.Empty;
                }
            }

            // --- СТИЛЬ 1: Имитация сетевого тайм-аута и резервного сервера ---
            private static string GenerateNetworkFallbackStyle()
            {
                string vSession = $"_0x{_rnd.Next(1000, 9999):x}";
                string vRetry = $"_0x{_rnd.Next(1000, 9999):x}";
                int fakePort = _rnd.Next(8000, 9000);

                var sb = new StringBuilder();
                sb.AppendLine($"\t\t\t\t// Проверка доступности резервного узла");
                // Условие всегда false, так как день недели не может быть 8-м
                sb.AppendLine($"\t\t\t\tif (DateTime.Now.DayOfWeek.ToString() == \"NeverDay\")");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\tstring {vSession} = \"fallback_session_id_\" + Guid.NewGuid().ToString();");
                sb.AppendLine($"\t\t\t\t\tint {vRetry} = ({_rnd.Next(100, 500)} * 3) / (DateTime.Now.Hour + 1);");
                sb.AppendLine($"\t\t\t\t\tif ({vRetry} > {fakePort}) {vSession} = \"disabled\";");
                sb.AppendLine("\t\t\t\t}");
                return sb.ToString();
            }

            // --- СТИЛЬ 2: Имитация проверки криптографического токена ---
            private static string GenerateCryptoValidationStyle()
            {
                string vBuffer = $"_0x{_rnd.Next(1000, 9999):x}";
                string vChecksum = $"_0x{_rnd.Next(1000, 9999):x}";

                var sb = new StringBuilder();
                sb.AppendLine($"\t\t\t\t// Валидация контрольной суммы пакета метаданных");
                // Условие всегда false, так как длина Хэша Guid всегда 36 символов
                sb.AppendLine($"\t\t\t\tif (Guid.NewGuid().ToString().Length < 10)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\tlong {vChecksum} = 0x{_rnd.Next(10000, 99999):X}L;");
                sb.AppendLine($"\t\t\t\t\tbyte[] {vBuffer} = BitConverter.GetBytes({vChecksum});");
                sb.AppendLine($"\t\t\t\t\tfor (int i = 0; i < {vBuffer}.Length; i++) {{ {vChecksum} ^= ({vBuffer}[i] << 2); }}");
                sb.AppendLine("\t\t\t\t}");
                return sb.ToString();
            }

            // --- СТИЛЬ 3: Имитация парсинга фейковой конфигурации ---
            private static string GenerateConfigParsingStyle()
            {
                string vRaw = $"_0x{_rnd.Next(1000, 9999):x}";
                string vCount = $"_0x{_rnd.Next(1000, 9999):x}";

                var sb = new StringBuilder();
                sb.AppendLine($"\t\t\t\t// Чтение локального кэша кастомизации окружения");
                // Условие всегда false, так как Environment.Version никогда не пустой
                sb.AppendLine($"\t\t\t\tif (Environment.Version.ToString() == string.Empty)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\tstring {vRaw} = \"node_count={_rnd.Next(10, 100)};mode=silent;theme=dark;\";");
                sb.AppendLine($"\t\t\t\t\tint {vCount} = {vRaw}.Split(';').Length;");
                sb.AppendLine($"\t\t\t\t\t{vRaw} = string.Concat({vRaw}, \"_processed_\", {vCount});");
                sb.AppendLine("\t\t\t\t}");
                return sb.ToString();
            }

            // --- СТИЛЬ 4: Имитация вычисления математических метрик системы ---
            private static string GenerateHardwareMetricsStyle()
            {
                string vFactor = $"_0x{_rnd.Next(1000, 9999):x}";
                string vResult = $"_0x{_rnd.Next(1000, 9999):x}";

                var sb = new StringBuilder();
                sb.AppendLine($"\t\t\t\t// Расчет динамического коэффициента масштабирования UI");
                // Условие всегда false, так как косинус 180 градусов (-1) никогда не будет равен 5
                sb.AppendLine($"\t\t\t\tif (Math.Cos(Math.PI) > 5.0)");
                sb.AppendLine("\t\t\t\t{");
                sb.AppendLine($"\t\t\t\t\tdouble {vFactor} = Math.Sqrt({_rnd.Next(50, 500)}) / {Environment.ProcessorCount};");
                sb.AppendLine($"\t\t\t\t\tdouble {vResult} = Math.Pow({vFactor}, 2) * Math.Log10({_rnd.Next(100, 1000)});");
                sb.AppendLine("\t\t\t\t}");
                return sb.ToString();
            }
            public static string GenerateChaosDeadCode()
            {
                var sb = new StringBuilder();

                // Генерируем уникальные имена фейковых переменных
                string v1 = "_0x" + _rnd.Next(1000, 9999).ToString("x");
                string v2 = "_0x" + _rnd.Next(1000, 9999).ToString("x");
                string v3 = "_0x" + _rnd.Next(1000, 9999).ToString("x");

                // Генерируем уникальное ложное условие
                string falseCondition = GenerateRandomFalseCondition();

                sb.AppendLine($"\t\t\t\tif ({falseCondition})");
                sb.AppendLine("\t\t\t\t{");

                // Случайный выбор внутренней логики
                int logicType = _rnd.Next(1, 4);
                if (logicType == 1)
                {
                    // Математический хаос
                    sb.AppendLine($"\t\t\t\t\tdouble {v1} = Math.Sqrt({_rnd.Next(10, 100)});");
                    sb.AppendLine($"\t\t\t\t\tint {v2} = (int){v1} * {_rnd.Next(2, 10)};");
                    sb.AppendLine($"\t\t\t\t\t{v1} = Math.Pow({v2}, {_rnd.Next(2, 4)});");
                }
                else if (logicType == 2)
                {
                    // Строковый хаос
                    sb.AppendLine($"\t\t\t\t\tstring {v1} = \"init_\" + {_rnd.Next(100, 999)};");
                    sb.AppendLine($"\t\t\t\t\tstring {v2} = {v1}.GetHashCode().ToString();");
                    sb.AppendLine($"\t\t\t\t\t{v1} = string.Concat({v1}, \"_\", {v2});");
                }
                else
                {
                    // Смешанный хаос с циклом
                    sb.AppendLine($"\t\t\t\t\tint {v1} = {_rnd.Next(5, 20)};");
                    sb.AppendLine($"\t\t\t\t\tfor (int {v2} = 0; {v2} < {v1}; {v2}++)");
                    sb.AppendLine("\t\t\t\t\t{");
                    sb.AppendLine($"\t\t\t\t\t\t{v1} += {_rnd.Next(1, 5)};");
                    sb.AppendLine("\t\t\t\t\t}");
                }

                sb.AppendLine("\t\t\t\t}");
                return sb.ToString();
            }

            private static string GenerateRandomFalseCondition()
            {
                int num1 = _rnd.Next(10, 100);
                int num2 = _rnd.Next(100, 500);
                int style = _rnd.Next(1, 5);

                switch (style)
                {
                    case 1:
                        // Пример: (53 * 2) % 2 == 1 -> Всегда false (четное число)
                        return $"({num1} * 2) % 2 == 1";
                    case 2:
                        // Пример: Math.Abs(-241) < 0 -> Всегда false
                        return $"Math.Abs(-{num2}) < 0";
                    case 3:
                        // Пример: Math.Sin(42) > 2.0 -> Синус не бывает больше 1. Всегда false
                        return $"Math.Sin({num1}) > 2.0";
                    case 4:
                        // Пример: "id_421".Length == 0 -> Всегда false
                        return $"\"id_{num2}\".Length == 0";
                    default:
                        return "1 == 2";
                }
            }
        }


        static void Main(string[] args)
        {
            // Твой метод Main остается прежним
            try
            {
                while (true)
                {
                    Console.WriteLine("Enter path with dev build:");
                    string appPath = Console.ReadLine().ToString();

                    if (!string.IsNullOrWhiteSpace(appPath) && Directory.Exists(appPath))
                    {
                        string targetPath = appPath.TrimEnd(Path.DirectorySeparatorChar) + "_Obfuscated";
                        bool needToCopy = true;

                        if (Directory.Exists(targetPath))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"\n[Warning] Folder already exists: {targetPath}");
                            Console.ResetColor();
                            Console.Write("Do you want to overwrite it? (y/n): ");
                            string answer = Console.ReadLine()?.Trim().ToLower() ?? "n";
                            if (answer == "y" || answer == "yes")
                            {
                                Directory.Delete(targetPath, true);
                            }
                            else
                            {
                                needToCopy = false;
                            }
                        }

                        if (needToCopy)
                        {
                            CloneFullProject(appPath, targetPath);
                        }

                        List<string> filesToProcess = GetProjectFiles(targetPath);

                        // Порядок проходов: сначала чистим приватные члены, потом локальные переменные, потом методы
                        ChangePrivateMembers(filesToProcess);
                        ChangeLocalString(filesToProcess);
                        ChangeGlobalMethods(filesToProcess);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nSuccess! Obfuscation step completed.");
                        Console.ResetColor();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Bad path. Try again.\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }

        static void ChangeLocalString(List<string> files)
        {
            foreach (var file in files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                string[] lines = File.ReadAllLines(file);
                var cleanedLines = new List<string>();
                bool insideBlockComment = false;
                bool isFileModified = false;
                bool isMethodStart = false;
                int open = 0;
                int close = 0;

                // Важно: локальный словарь живет ТОЛЬКО внутри одного метода, 
                // чтобы не переименовать что-то лишнее в соседних методах.
                var localRenameRules = new Dictionary<string, string>();

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    // Стриппинг комментариев (оставляем твою логику)
                    if (insideBlockComment)
                    {
                        isFileModified = true;
                        int blockEndIndex = line.IndexOf("*/");
                        if (blockEndIndex != -1) { line = line.Substring(blockEndIndex + 2); insideBlockComment = false; }
                        else { cleanedLines.Add(string.Empty); continue; }
                    }

                    int blockStartIndex = line.IndexOf("/*");
                    if (blockStartIndex != -1)
                    {
                        isFileModified = true;
                        int blockEndIndex = line.IndexOf("*/", blockStartIndex + 2);
                        if (blockEndIndex != -1) line = line.Remove(blockStartIndex, (blockEndIndex + 2) - blockStartIndex);
                        else { line = line.Substring(0, blockStartIndex); insideBlockComment = true; }
                    }

                    // Очистка однострочных комментариев
                    int singleCommentIndex = line.IndexOf("//");
                    while (singleCommentIndex != -1)
                    {
                        string textBeforeComment = line.Substring(0, singleCommentIndex);
                        int quoteCount = textBeforeComment.Count(c => c == '"');
                        if (quoteCount % 2 != 0) singleCommentIndex = line.IndexOf("//", singleCommentIndex + 2);
                        else { line = textBeforeComment; isFileModified = true; break; }
                    }

                    // Отслеживаем границы методов
                    // Добавили проверку на private/public/internal, чтобы точнее ловить начало методов, а не их вызовы
                    if (Regex.IsMatch(line, @"\b(void|Task|string|int|bool)\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\("))
                    {
                        isMethodStart = true;
                        localRenameRules.Clear(); // Очищаем правила для нового метода!
                    }

                    if (isMethodStart)
                    {
                        int openInLine = line.Count(c => c == '{');
                        int closeInLine = line.Count(c => c == '}');
                        open += openInLine;
                        close += closeInLine;

                        // Улучшенная регулярка для локальных переменных:
                        // Ищет строго внутри методов объявления типа: "тип имя = значение;" или "type name;"
                        // Исключает попадание свойств (содержащих get/set) и полей WMI var obj в циклах forech
                        string variablePattern = @"\b(string|int|bool|var)\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*(;|=)";
                        MatchCollection matches = Regex.Matches(line, variablePattern);

                        foreach (Match match in matches)
                        {
                            string variableName = match.Groups[2].Value;

                            // Проверяем, что это не системное слово и не var из foreach узла WMI
                            if (!blacklist.Contains(variableName) && !localRenameRules.ContainsKey(variableName) && variableName != "obj")
                            {
                                string hexName = GenerateRandomHexName();
                                localRenameRules[variableName] = hexName;
                            }
                        }

                        // Применяем локальные правила замены
                        foreach (var rule in localRenameRules)
                        {
                            string oldName = rule.Key;
                            string newName = rule.Value;

                            // Жесткое ограничение: переменная не должна идти после точки (obj.Name) 
                            // и перед ней не должно быть ключевых слов типа объекта var, obj, или кавычек
                            string safePattern = @"(?<![\.\""\w])\b" + Regex.Escape(oldName) + @"\b(?![\""\w])";

                            if (Regex.IsMatch(line, safePattern))
                            {
                                line = Regex.Replace(line, safePattern, newName);
                                isFileModified = true;
                            }
                        }

                        if (open > 0 && close >= open)
                        {
                            isMethodStart = false;
                            open = 0;
                            close = 0;
                            localRenameRules.Clear(); // Вышли из метода — забыли локальные переменные
                        }
                    }

                    cleanedLines.Add(line.TrimEnd());
                }

                if (isFileModified)
                {
                    File.WriteAllLines(file, cleanedLines);
                    Console.WriteLine($"[File rewrite] {Path.GetFileName(file)}");
                }
            }
        }

        static void ChangeGlobalMethods(List<string> files)
        {
            // Глобальный словарь для методов: [Старое имя] -> [Новое Hex-имя]
            var methodRenameRules = new Dictionary<string, string>();

            // Регулярка ищет объявление метода: модификатор -> тип возвращаемого значения -> имя -> открывающая скобка
            // Примеры: public void MyMethod(, private static string GetId(
            // Защита (?!\s*if|\s*switch|\s*while|\s*using) исключает ложные срабатывания на ключевые слова
            string methodDeclarationPattern = @"\b(public|private|internal|protected)\s+(static\s+|virtual\s+|override\s+|async\s+)?([a-zA-Z0-9_<>]+\s+)([a-zA-Z_][a-zA-Z0-9_]*)\s*\((?!\s*if|\s*switch|\s*while|\s*using)";
            Regex declRegex = new Regex(methodDeclarationPattern, RegexOptions.Compiled);

            // --- ПРОХОД 1: Сбор всех методов в проекте ---
            foreach (var file in files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                // Читаем файл (здесь ты можешь использовать уже очищенные от комментариев строки)
                string[] lines = File.ReadAllLines(file);

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();

                    // Пропускаем директивы usings и namespaces
                    if (trimmed.StartsWith("using ", StringComparison.Ordinal) ||
                        trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                        continue;
                    Match match = declRegex.Match(line);
                    if (match.Success)
                    {
                        string methodName = match.Groups[4].Value; // Наше имя метода

                        // 1. ПРОВЕРКА: Пропускаем, если метод в черном списке
                        if (methodBlacklist.Contains(methodName) || blacklist.Contains(methodName)) // <--- ДОБАВЛЕН blacklist
                            continue;

                        // 2. ПРОВЕРКА: Пропускаем обработчики событий WPF окон (всё, что заканчивается на _Click, _KeyDown, _Loaded и т.д.)
                        if (methodName.Contains("_Click") ||
                            methodName.Contains("_Mouse") ||
                            methodName.Contains("_Key") ||
                            methodName.Contains("_Loaded"))
                        {
                            continue;
                        }
                        if (line.Contains("object sender"))
                        {
                            continue;
                        }

                        // 3. Дополнительная страховка по суффиксам (на случай, если sender переименован)
                        if (methodName.Contains("_Click") || methodName.Contains("_Mouse") ||
                            methodName.Contains("_Key") || methodName.Contains("_Loaded"))
                        {
                            continue;
                        }
                        // Только если метод прошел проверки — добавляем в словарь правил
                        if (!methodRenameRules.ContainsKey(methodName))
                        {
                            string hexName = GenerateRandomHexName();
                            methodRenameRules[methodName] = hexName;
                            Console.WriteLine($"[Method Collected - SAFE] {methodName} -> {hexName}");
                        }
                    }
                }
            }

            // --- ПРОХОД 2: Тотальная замена вызовов методов по всему проекту ---
            if (methodRenameRules.Count > 0)
            {
                foreach (var file in files)
                {
                    string content = File.ReadAllText(file);
                    bool isModified = false;

                    // Обработка C# файлов
                    if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var rule in methodRenameRules)
                        {
                            // Защита (?<!\.) тут НЕ НУЖНА, так как методы часто вызываются через точку (например, obj.MyMethod())
                            // Но нам нужны четкие границы слова (\b)
                            string methodPattern = @"\b" + Regex.Escape(rule.Key) + @"\b";

                            if (Regex.IsMatch(content, methodPattern))
                            {
                                content = Regex.Replace(content, methodPattern, rule.Value);
                                isModified = true;
                            }
                        }
                    }
                    // Обработка XAML (на случай если методы привязаны через Event'ы, например Click="Button_Click")
                    else if (file.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var rule in methodRenameRules)
                        {
                            string methodPattern = @"\b" + Regex.Escape(rule.Key) + @"\b";

                            if (Regex.IsMatch(content, methodPattern))
                            {
                                content = Regex.Replace(content, methodPattern, rule.Value);
                                isModified = true;
                            }
                        }
                    }

                    // Перезаписываем файл, если были изменения
                    if (isModified)
                    {
                        File.WriteAllText(file, content);
                        Console.WriteLine($"[Methods Updated] {Path.GetFileName(file)}");
                    }
                }
            }
        }

        static void ChangePrivateMembers(List<string> files)
        {
            // 1. Регулярка для приватных МЕТОДОВ: private -> тип -> имя -> скобка (
            string methodPattern = @"\bprivate\s+(static\s+|async\s+)?([a-zA-Z0-9_<>]+\s+)([a-zA-Z_][a-zA-Z0-9_]*)\s*\(";
            Regex methodRegex = new Regex(methodPattern, RegexOptions.Compiled);

            // 2. Регулярка для приватных ПЕРЕМЕННЫХ/ПОЛЕЙ: private -> тип -> имя -> точка с запятой ; (или знак =)
            string fieldPattern = @"\bprivate\s+(static\s+|readonly\s+|const\s+)?([a-zA-Z0-9_<>]+\s+)([a-zA-Z_][a-zA-Z0-9_]*)\s*(;|=)";
            Regex fieldRegex = new Regex(fieldPattern, RegexOptions.Compiled);

            foreach (var file in files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                string[] lines = File.ReadAllLines(file);
                bool isModified = false;

                // Локальный словарь для ВСЕХ приватных членов текущего файла
                var privateRenameRules = new Dictionary<string, string>();

                // --- ШАГ 1: Построчный сбор приватных методов и переменных ---
                for (int i = 0; i < lines.Length; i++)
                {
                    string trimmed = lines[i].Trim();

                    // Пропускаем системные директивы usings и namespaces
                    if (trimmed.StartsWith("using ", StringComparison.Ordinal) ||
                        trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                        continue;

                    // Сначала проверяем на приватный метод
                    Match methodMatch = methodRegex.Match(lines[i]);
                    if (methodMatch.Success)
                    {
                        string methodName = methodMatch.Groups[3].Value; // Группа 3 — имя метода

                        // Защита от системных событий WPF (object sender)
                        if (lines[i].Contains("object sender") || methodName.Contains("_Click") || methodName.Contains("_Mouse"))
                            continue;

                        if (!privateRenameRules.ContainsKey(methodName) && methodName != "Main" && !blacklist.Contains(methodName))
                        {
                            string hexName = GenerateRandomHexName();
                            privateRenameRules[methodName] = hexName;
                            Console.WriteLine($"[Private Method] {methodName} -> {hexName} in {Path.GetFileName(file)}");
                        }
                        continue; // Строка обработана, переходим к следующей
                    }

                    // Если это не метод, проверяем на приватную переменную (поле класса)
                    Match fieldMatch = fieldRegex.Match(lines[i]);
                    if (fieldMatch.Success)
                    {
                        string fieldName = fieldMatch.Groups[3].Value; // Группа 3 — имя переменной

                        if (!privateRenameRules.ContainsKey(fieldName) && !blacklist.Contains(fieldName))
                        {
                            string hexName = GenerateRandomHexName();
                            privateRenameRules[fieldName] = hexName;
                            Console.WriteLine($"[Private Field] {fieldName} -> {hexName} in {Path.GetFileName(file)}");
                        }
                    }
                }

                // --- ШАГ 2: Построчная замена вызовов и использований ---
                if (privateRenameRules.Count > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string trimmed = lines[i].Trim();
                        if (trimmed.StartsWith("using ", StringComparison.Ordinal) ||
                            trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                            continue;

                        foreach (var rule in privateRenameRules)
                        {
                            // Ищем точное слово целиком. 
                            // Точку (?<!\.) НЕ запрещаем, так как приватные поля часто вызываются как: this.myField или _viewModel.myField
                            string wordPattern = @"\b" + Regex.Escape(rule.Key) + @"\b";

                            if (Regex.IsMatch(lines[i], wordPattern))
                            {
                                lines[i] = Regex.Replace(lines[i], wordPattern, rule.Value);
                                isModified = true;
                            }
                        }
                    }
                }

                // Если в файле были изменения — сохраняем
                if (isModified)
                {
                    File.WriteAllLines(file, lines);
                    Console.WriteLine($"[Saved] Obfuscated private members in: {Path.GetFileName(file)}");
                }
            }
        }

        static void ChangeGlobalPropertiesAndBindings(List<string> files)
        {
            // 1. Черный список системных имен WPF и .NET создаем СРАЗУ
            var systemProtected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Path", "Convert", "Message", "ToString", "Equals", "GetHashCode", "GetType",
        "Data", "Value", "Id", "Name", "Text", "Count", "Length", "Item", "Type",
        "Execute", "CanExecute", "Invoke", "ConvertBack", "CanExecuteChanged",
        "MainWindow", "Current", "Application", "Resources", "InitializeComponent", "App",
        "PropertyChanged", "SwitchTab", "Instance", "Tag", "Content", "DataContext", "sender", "e"
    };

            var globalRenameRules = new Dictionary<string, string>();

            // ЭТАП 1: АНАЛИЗ И СБОР ИМЕН В .CS ФАЙЛАХ
            foreach (var file in files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[Analyzing File] -> {Path.GetFileName(file)}");
                Console.ResetColor();

                string[] lines = File.ReadAllLines(file);
                bool insideClass = false;
                int classBraceDepth = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string trimmed = line.Trim();

                    if (trimmed.StartsWith("using ", StringComparison.Ordinal) ||
                        trimmed.StartsWith("namespace ", StringComparison.Ordinal))
                        continue;

                    if (trimmed.Contains("class "))
                    {
                        insideClass = true;
                        classBraceDepth = 0;

                        if (trimmed.Contains(": Page") || trimmed.Contains(": Window") || trimmed.Contains(": UserControl") || trimmed.Contains(": Application"))
                        {
                            var classMatch = Regex.Match(trimmed, @"class\s+([a-zA-Z_][a-zA-Z0-9_]*)");
                            if (classMatch.Success)
                            {
                                string wpfClassName = classMatch.Groups[1].Value;
                                systemProtected.Add(wpfClassName); // Защищаем само имя класса страницы/окна!
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"   [Protected WPF Component Class]: {wpfClassName}");
                                Console.ResetColor();
                            }
                        }
                    }

                    if (insideClass)
                    {
                        int openInLine = trimmed.Count(c => c == '{');
                        int closeInLine = trimmed.Count(c => c == '}');

                        if (classBraceDepth <= 1 && trimmed.StartsWith("public ", StringComparison.Ordinal))
                        {
                            // ГЛУБОКАЯ ФИЛЬТРАЦИЯ: Пропускаем события (events), чтобы не сломать INotifyPropertyChanged
                            if (!trimmed.Contains(" event ") && !trimmed.Contains("\tevent "))
                            {
                                // Регулярка теперь ищет ТОЛЬКО свойства, поля и методы (игнорирует объявление класса)
                                var match = Regex.Match(trimmed, @"\bpublic\s+(?!class\b)(?:[a-zA-Z0-9_<>\?\[\]]+\s+)+([a-zA-Z_][a-zA-Z0-9_]*)\b");

                                if (match.Success)
                                {
                                    string publicName = match.Groups[1].Value;
                                    string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file);

                                    if (publicName != "class" && publicName != fileNameWithoutExt && publicName != "void")
                                    {
                                        // ИСПРАВЛЕНИЕ: Пропускаем системные имена, а также типы классов Converter и ViewModel
                                        if (systemProtected.Contains(publicName) ||
                                            publicName.EndsWith("Converter", StringComparison.OrdinalIgnoreCase) ||
                                            publicName.EndsWith("ViewModel", StringComparison.OrdinalIgnoreCase))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine($"   [Skipped System Contract/Type]: {publicName} (Line {i + 1})");
                                            Console.ResetColor();
                                        }
                                        else if (!globalRenameRules.ContainsKey(publicName))
                                        {
                                            string obfuscatedName = "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                                            globalRenameRules.Add(publicName, obfuscatedName);
                                            Console.WriteLine($"   [Registered Member]: {publicName} -> {obfuscatedName}");
                                        }
                                    }
                                }
                            }
                        }

                        classBraceDepth += openInLine - closeInLine;
                        if (classBraceDepth < 0) insideClass = false;
                    }
                }
            }
        }





        static void CollectGlobalStringVariables(List<string> files)
        {
            // 1. Регулярка для полей класса (public string MyVar;)
            string fieldPattern = @"\b(public|private|internal|protected)\s+(static\s+|readonly\s+)?string\s+([a-zA-Z_][a-zA-Z0-9_]*)\b";
            Regex fieldRegex = new Regex(fieldPattern, RegexOptions.Compiled);

            // 2. Регулярка для методов, возвращающих string (public static string MyMethod(args) )
            // Отличается тем, что после имени ОБЯЗАТЕЛЬНО идут круглые скобки (с аргументами или без)
            string methodPattern = @"\b(public|private|internal|protected)\s+(static\s+|virtual\s+|override\s+)?string\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(";
            Regex methodRegex = new Regex(methodPattern, RegexOptions.Compiled);

            foreach (var file in files.Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                string[] lines = File.ReadAllLines(file);
                bool insideMethod = false;
                int braceCounter = 0;

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed.StartsWith("using ") || trimmed.StartsWith("namespace "))
                        continue;

                    // Трекинг фигурных скобок для определения контекста класса/метода
                    if (trimmed.Contains("{")) { braceCounter++; }
                    if (trimmed.Contains("}")) { braceCounter--; }
                    insideMethod = braceCounter > 1;

                    // --- ИЩЕМ МЕТОДЫ (Их ищем везде, так как они объявляются на уровне класса) ---
                    Match methodMatch = methodRegex.Match(line);
                    if (methodMatch.Success)
                    {
                        string methodName = methodMatch.Groups[3].Value; // 3-я группа — это имя метода

                        if (!blacklist.Contains(methodName) && !globalRenameRules.ContainsKey(methodName))
                        {
                            string hexName = GenerateRandomHexName();
                            globalRenameRules.Add(methodName, hexName);
                            Console.WriteLine($"Found STRING METHOD: '{methodName}' -> '{hexName}'");
                            continue; // Если нашли метод, поле в этой строке искать уже не нужно
                        }
                    }

                    // --- ИЩЕМ ПОЛЯ (Только вне методов) ---
                    if (!insideMethod)
                    {
                        Match fieldMatch = fieldRegex.Match(line);
                        if (fieldMatch.Success)
                        {
                            string fieldName = fieldMatch.Groups[3].Value; // 3-я группа — имя поля

                            if (!blacklist.Contains(fieldName) && !globalRenameRules.ContainsKey(fieldName))
                            {
                                string hexName = GenerateRandomHexName();
                                globalRenameRules.Add(fieldName, hexName);
                                Console.WriteLine($"Found GLOBAL field: '{fieldName}' -> '{hexName}'");
                            }
                        }
                    }
                }
            }
        }

        static string GenerateRandomHexName()
        {
            while (true)
            {
                byte[] buffer = new byte[2];
                RandomNumberGenerator.Fill(buffer);
                string newName = "_0x" + Convert.ToHexString(buffer).ToLower();
                if (usedHexNames.Add(newName)) return newName;
            }
        }
        /// <summary>
        /// Копирует проект полностью (картинки, ресурсы, настройки), отсекая только кэш-папки.
        /// </summary>
        static void CloneFullProject(string sourceDir, string targetDir)
        {
            var bannedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", ".vs", "bin", "obj" };

            foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, dirPath);
                string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);

                if (pathParts.Any(part => bannedFolders.Contains(part))) continue;

                Directory.CreateDirectory(Path.Combine(targetDir, relativePath));
            }

            foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, filePath);
                string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar);

                if (pathParts.Any(part => bannedFolders.Contains(part))) continue;

                string destFile = Path.Combine(targetDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(filePath, destFile, true);
            }
        }

        /// <summary>
        /// Ищет файлы .cs и .xaml внутри созданной копии
        /// </summary>
        static List<string> GetProjectFiles(string rootPath)
        {
            var foundFiles = new List<string>();
            var ignoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".git", ".vs", "bin", "obj" };
            var foldersToScan = new Queue<string>();
            foldersToScan.Enqueue(rootPath);

            while (foldersToScan.Count > 0)
            {
                string currentFolder = foldersToScan.Dequeue();
                try
                {
                    foreach (string file in Directory.EnumerateFiles(currentFolder, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        if (ext == ".cs" || ext == ".xaml") foundFiles.Add(file);
                    }
                    foreach (string subDir in Directory.EnumerateDirectories(currentFolder, "*", SearchOption.TopDirectoryOnly))
                    {
                        if (!ignoredFolders.Contains(Path.GetFileName(subDir))) foldersToScan.Enqueue(subDir);
                    }
                }
                catch (UnauthorizedAccessException) { }
            }
            return foundFiles;
        }
    }
}
