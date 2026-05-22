using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace WPF_Hide_Names
{
    internal class Program
    {
        // 1. Глобальный словарь: [Старое имя] -> [Новое Hex-имя]
        // Сюда пойдут имена полей и свойств классов, которые используются в XAML и других файлах
        static Dictionary<string, string> globalRenameRules = new Dictionary<string, string>();

        // Сет для контроля уникальности всех сгенерированных Hex-имен (чтобы не было дублей)
        static HashSet<string> usedHexNames = new HashSet<string>();

        static HashSet<string> blacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Class", "Name", "Command", "Binding", "Path", "String", "Main", "App", "Application",
            "System", "Windows", "Controls", "Data", "Visibility", "True", "False", "Null"
        };

        static HashSet<string> methodBlacklist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "Main", "InitializeComponent", "OnStartup", "OnExit", "CanExecute", "Execute",
    "Convert", "ConvertBack", "Initialize", "OnPropertyChanged", "RefreshUI"
};
        static void Main(string[] args)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine("Enter path with dev build:");
                    string appPath = @"C:\Users\lxlyu\source\repos\BulsBust0.1.0";
                    //string appPath = Console.ReadLine() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(appPath) && Directory.Exists(appPath))
                    {
                        // 1. Создаем полный путь для копии проекта по соседству
                        string targetPath = appPath.TrimEnd(Path.DirectorySeparatorChar) + "_Obfuscated";
                        bool needToCopy = true;

                        // ПРОВЕРКА: Если папка уже существует
                        if (Directory.Exists(targetPath))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"\n[Warning] Folder already exists: {targetPath}");
                            Console.ResetColor();
                            Console.Write("Do you want to overwrite it? (y/n): ");

                            string answer = Console.ReadLine()?.Trim().ToLower() ?? "n";
                            if (answer == "y" || answer == "yes")
                            {
                                Console.WriteLine("Deleting old copy...");
                                Directory.Delete(targetPath, true);
                            }
                            else
                            {
                                needToCopy = false;
                                Console.WriteLine("Using existing copy without re-cloning.");
                            }
                        }

                        // Копируем, только если папки не было или пользователь согласился перезаписать
                        if (needToCopy)
                        {
                            Console.WriteLine($"\n[1/5] Cloning full project structure...");
                            CloneFullProject(appPath, targetPath);
                            Console.WriteLine("Project successfully cloned!");
                        }

                        // 2. Ищем файлы, которые мы будем модифицировать, внутри копии
                        Console.WriteLine("\n[2/5] Scanning target files for obfuscation...");
                        List<string> filesToProcess = GetProjectFiles(targetPath);
                        Console.WriteLine($"Found {filesToProcess.Count} source files (.cs/.xaml) ready for changes.");

                        // 3. ПЕРВЫЙ ПРОХОД: задаём локлаьным переменным хэш имена
                        Console.WriteLine("\n[3/5] Analyzing files and collecting 'string' variables...");
                        ChangeLocalString(filesToProcess);
                        Console.WriteLine($"Collected {globalRenameRules.Count} unique global variables.");

                        // 4. ВТОРОЙ ПРОХОД: задаём глобальным переменным хэш имена
                        Console.WriteLine("\n[4/5] Analyzing files and collecting 'methods' variables...");
                        ChangeGlobalMethods(filesToProcess);
                        Console.WriteLine($"Collected {globalRenameRules.Count} unique global variables.");

                        // 5. ТРЕТИЙ ПРОХОД: Применяем изменения и перезаписываем файлы
                        //Console.WriteLine("\n[4/4] Applying obfuscation and saving files...");
                        //ApplySafeObfuscation(filesToProcess);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nSuccess! Obfuscation step completed. You can check the files now.");
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
                var cleanedLines = new List<string>();//массив строк фаила
                bool insideBlockComment = false; //внутри блока?
                bool isFileModified = false;//были ли изменения фаила?
                bool isMethodStart = false; //находимся ли мы в методе?
                int open = 0; //сколько открытых скобочек
                int close = 0;//сколько закрытых скобочек

                var localRenameRules = new Dictionary<string, string>();//локальныи словарь строк
                for (int i = 0; i < lines.Length; i++)
                {
                    string originalLine = lines[i];
                    string line = originalLine;

                    // 1. Обработка многострочного комментария /* ... */ (если мы уже внутри него)
                    if (insideBlockComment)
                    {
                        isFileModified = true;
                        int blockEndIndex = line.IndexOf("*/");
                        if (blockEndIndex != -1)
                        {
                            line = line.Substring(blockEndIndex + 2);
                            insideBlockComment = false;
                        }
                        else
                        {
                            // Если вся строка внутри комметария, заменяем её пустой строкой
                            cleanedLines.Add(string.Empty);
                            continue;
                        }
                    }

                    // 2. Поиск начала нового блочного комментария /*
                    int blockStartIndex = line.IndexOf("/*");
                    if (blockStartIndex != -1)
                    {
                        isFileModified = true;
                        int blockEndIndex = line.IndexOf("*/", blockStartIndex + 2);
                        if (blockEndIndex != -1)
                        {
                            line = line.Remove(blockStartIndex, (blockEndIndex + 2) - blockStartIndex);
                        }
                        else
                        {
                            line = line.Substring(0, blockStartIndex);
                            insideBlockComment = true;
                        }
                    }

                    // 3. Безопасное удаление однострочного комментария //
                    int singleCommentIndex = line.IndexOf("//");
                    while (singleCommentIndex != -1)
                    {
                        string textBeforeComment = line.Substring(0, singleCommentIndex);
                        int quoteCount = textBeforeComment.Count(c => c == '"');

                        if (quoteCount % 2 != 0)
                        {
                            singleCommentIndex = line.IndexOf("//", singleCommentIndex + 2);
                        }
                        else
                        {
                            line = textBeforeComment;
                            isFileModified = true;
                            break;
                        }
                    }


                    if (Regex.IsMatch(line, @"\bvoid\b") || Regex.IsMatch(line, @"\bTask\b"))
                    {
                        isMethodStart = true;
                    }
                    if (isMethodStart)
                    {
                        int openInLine = line.Count(c => c == '{');
                        int closeInLine = line.Count(c => c == '}');
                        open += openInLine;
                        close += closeInLine;


                        string variablePattern = @"\b(string|int)\s+([a-zA-Z_][a-zA-Z0-9_]*)\b(?!\s*\()";
                        MatchCollection matches = Regex.Matches(line, variablePattern);

                        foreach (Match match in matches)
                        {
                            string variableName = match.Groups[2].Value;

                            if (!localRenameRules.ContainsKey(variableName))
                            {
                                string hexName = GenerateRandomHexName();

                                localRenameRules[variableName] = hexName;

                            }
                        }

                        foreach (var rule in localRenameRules)
                        {
                            string oldName = rule.Key;
                            string newName = rule.Value;

                            // Ищем имя переменной целиком (\b), но только если перед ним НЕТ точки (?<!\.)
                            string safePattern = @"(?<!\.)\b" + Regex.Escape(oldName) + @"\b";

                            if (Regex.IsMatch(line, safePattern))
                            {
                                line = Regex.Replace(line, safePattern, newName);
                                isFileModified = true; // Фиксируем, что файл был изменен
                            }
                        }

                        if (open > 0)
                        {
                            if (close == open)
                            {
                                isMethodStart = false; // Выключаем режим метода
                                open = 0;              // Сбрасываем счетчики для следующего метода
                                close = 0;
                            }
                        }
                    }


                    // 4. Добавляем итоговую строку (даже если она стала пустой) в наш список
                    // TrimEnd убирает пробелы, которые могли остаться перед удаленным комментарием
                    cleanedLines.Add(line.TrimEnd());
                }

                // 5. Перезаписываем файл, ТОЛЬКО если в нем действительно были комментарии и код изменился
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
                        if (methodBlacklist.Contains(methodName))
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
                // Генерируем 2 случайных байта (это даст 4 шестнадцатеричных символа)
                byte[] buffer = new byte[2];
                System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);

                // Превращаем в строку вида _0x4f2a
                string newName = "_0x" + Convert.ToHexString(buffer).ToLower();

                // Если такое имя ЕЩЕ НЕ использовалось в проекте, занимаем его и возвращаем
                if (usedHexNames.Add(newName))
                {
                    return newName;
                }
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
