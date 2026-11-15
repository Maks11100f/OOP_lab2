# ЛР 2* — Робота з файлами XML (варіант 5 «Розклад»)

Застосунок .NET MAUI (Windows) для аналізу XML-файлу розкладу занять:
пошук за значеннями атрибутів трьома способами (SAX, DOM, LINQ to XML),
трансформація у HTML через XSLT, збереження відфільтрованого фрагмента
на Google Drive у форматах XML і HTML та протоколювання подій.

## Шаблони проєктування

| Шаблон | Класи |
|---|---|
| **Стратегія** | `IXmlSearchStrategy`, `SaxSearchStrategy`, `DomSearchStrategy`, `LinqSearchStrategy`, `XmlSearchContext` |
| **Фабричний метод** | `FragmentSaver` (метод `CreateWriter()`), `XmlFragmentSaver`, `HtmlFragmentSaver`, `IFragmentWriter`, `XmlFragmentWriter`, `HtmlFragmentWriter` |
| **Одинак** | `Logger` (+ `LogLevel`: Filtration / Transformation / Saving) |

## Структура проєкту

```
Models/       Lesson, ScheduleXml (структура XML)
Services/     XmlAttributeExtractor, XslTransformer, GoogleDriveService
Strategies/   шаблон «Стратегія»
Saving/       шаблон «Фабричний метод»
Logging/      шаблон «Одинак»
Resources/Raw schedule.xml, schedule.xsl (+ ваш credentials.json)
```

## Вхідний XML

```xml
<Schedule>
  <Lesson Teacher="..." Faculty="..." Department="..." Room="303"
          Subject="..." Group="К-25" Day="Понеділок" Time="08:40">
    <Students>
      <Student Name="..." />
    </Students>
  </Lesson>
</Schedule>
```

Кількість атрибутів у вузлах `Lesson` різна (онлайн-заняття не мають `Room`).

## Налаштування Google Drive

1. [Google Cloud Console](https://console.cloud.google.com/) → створіть проєкт.
2. *APIs & Services → Library* → увімкніть **Google Drive API**.
3. *OAuth consent screen* → тип **External**, додайте свій акаунт у **Test users**.
4. *Credentials → Create credentials → OAuth client ID* → тип **Desktop app**.
5. Завантажте JSON і збережіть його як `Resources/Raw/credentials.json`.

При першому зверненні до Google Drive відкриється браузер для входу.
`credentials.json` додано до `.gitignore` — у репозиторій він не потрапляє.

## Де лежать файли

- Журнал подій: `Документи\OOP_lab2\log.txt`
- Згенеровані HTML/XML: `Документи\OOP_lab2\`

Приклад журналу:

```
15.11.2026 13:02:22 Фільтрація. Параметри: «Teacher» – Іваненко Олег Петрович. Спосіб: SAX API. Знайдено: 2
15.11.2026 13:02:40 Трансформація. Збережено у файл fragment.html
15.11.2026 13:03:05 Збереження. Збережено відфільтрований фрагмент у файл fragment.xml
```
