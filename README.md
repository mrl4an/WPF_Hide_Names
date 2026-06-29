# WPF_Hide_Names
# WPF_Hide_Names

A custom source-code obfuscator designed for **WPF (C#)** applications.

The tool copies your project, strips out temporary "junk" files and developer comments, and scrambles component names to confuse decompilers. The main advantage of this source-level method is **zero false positives (F/P) from antivirus software**, as the final output is clean, valid C# code.

## 🚀 How It Works

1. **Copying**: The utility physically copies all your project files to a new directory.
2. **Filtering**: It automatically ignores folders and files that are unnecessary for building (`bin`, `obj`, `.git`, `.vs`, etc.).
3. **Cleaning**: All developer comments are automatically stripped from the source code.
4. **Obfuscation**: It replaces the names of classes, methods, and variables with randomized or scrambled identifiers.

> ⚠️ **Important**: Minimal manual code fixing might be required after obfuscation. The automated renaming process may accidentally affect strings used in serialization/deserialization (e.g., JSON model fields).

## ✨ Features & Advantages

- **FUD (Fully Undetected)**: Antivirus scanners will not flag your compiled binary because it is built entirely from scratch.
- **Clean Output**: No third-party DLLs, complex loaders, or leftover comments in the final code.
- **Open Source**: Completely free to use, modify, and improve.

## 🛠 Tech Stack

- **Language**: C#
- **Platform**: .NET / ConsoleApp

## 📋 To-Do List

The project is evolving, and contributions are welcome. Future goals include:
- [ ] Implement automatic exclusion for model classes (JSON/Database entities) to prevent renaming errors.

## 🤝 Contributing

Pull requests are highly welcome! If you find a bug or have a feature request:
1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/AmazingFeature`).
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4. Push to the branch (`git push origin feature/AmazingFeature`).
5. Open a Pull Request.

## 📄 License

Distributed under the MIT License.

Самописный обфускатор исходного кода для приложений **WPF (C#)**. 

Инструмент копирует проект, очищает его от "мусора" и комментариев, а также изменяет имена компонентов, путая логику для декомпиляторов. Главное преимущество метода — **полное отсутствие ложных срабатываний (F/P) со стороны антивирусов**, так как на выходе получается чистый, валидный C# код.

## 🚀 Как это работает

1. **Копирование**: Утилита физически копирует все файлы вашего проекта в новую директорию.
2. **Фильтрация**: Игнорируются ненужные для сборки папки и файлы (`bin`, `obj`, `.git`, `.vs` и т.д.).
3. **Очистка**: Из исходного кода автоматически удаляются все комментарии разработчика.
4. **Обфускация**: Происходит автоматическая замена имен классов, методов и переменных на запутанные значения.

> ⚠️ **Важно**: После работы обфускатора может потребоваться минимальная ручная правка кода. Автоматическая замена может затронуть строки, используемые в сериализации/десериализации (например, поля JSON-моделей).

## ✨ Преимущества

- **FUD (Fully Undetected)**: Антивирусы не ругаются на готовый бинарник, так как компиляция идет с чистого листа.
- **Чистота**: Никаких сторонних DLL, сложных загрузчиков (loaders) и лишних комментариев в коде.
- **Open Source**: Проект полностью бесплатный, код открыт для модификаций и улучшений.

## 🛠 Технологический стек

- **Язык**: C#
- **Платформа**: .NET / Console App

## 📋 План развития (To-Do)

Проект активно развивается. Вы можете помочь с реализацией следующих фич:
- [ ] Автоматическое исключение классов моделей (JSON/Базы данных) из процесса переименования.

## 🤝 Вклад в развитие (Contributing)

Пулл-реквесты приветствуются! Если вы нашли баг или хотите добавить новую фичу:
1. Сделайте форк репозитория.
2. Создайте ветку для ваших изменений (`git checkout -b feature/AmazingFeature`).
3. Закоммитьте изменения (`git commit -m 'Add some AmazingFeature'`).
4. Отправьте ветку в свой форк (`git push origin feature/AmazingFeature`).
5. Откройте Pull Request.

## 📄 Лицензия

Распространяется под лицензией MIT.
