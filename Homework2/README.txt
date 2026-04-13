Домашнее задание №2, вариант 18.

Предметная область:
- Справочник: Кафедры
- Основная таблица: Преподаватели
- Числовое поле: publications

Состав проекта:
- Chair.cs
- Teacher.cs
- DatabaseManager.cs
- ReportBuilder.cs
- Program.cs
- chair.csv
- teacher.csv
- 3 схемы PlantUML
- Homework2_Variant18_CSharp.csproj

Запуск:
1. Открыть папку проекта в Visual Studio / Rider / VS Code.
2. Восстановить NuGet-пакеты.
3. Запустить проект.

При первом запуске:
- создаётся SQLite-база teachers.db
- загружаются данные из chair.csv и teacher.csv

В проект также включены дополнительные возможности из методички:
- Numbered()
- Footer(...)
- SaveToFile(...)
- фильтр по кафедре
- экспорт CSV
