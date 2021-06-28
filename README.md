# SessionLicenseControl
### [ENG](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/EN.md)

Проект предназначен для учёта сессий, и контроля лицензии в ПО
При запуске считываются данные из зашифрованного архива и проверяются в зависимости от требований.

библиотека создана при поддержке https://github.com/Infarh

### Быстрый старт:

`Install-SessionLicenseControl -Version 1.0.0`

`Install-Package SessionLicenseControl.WPF -Version 1.0.0 For WPF`

для работы с лицензией:
создание:

```C#
  var lic = new LicenseGenerator(Secret, hdd, expirationDate, Ouner, check_sessions);
  lic.CreateLicenseFile(LicenseFilePath);
```
где:

* `Secret` - Секретная последовательность для шифрования данных
* `hdd` - Идентификатор жесткого диска компьютера, на котором будет использоваться файл
* `expirationDate` - Дата истечения срока действия лицензии
* `Ouner` - Для кого создана лицензия
* `check_sessions` - Включить управление сеансами в лицензиях
* `LicenseFilePath` - Путь, в котором будет сохранен файл лицензии

`Вы можете использовать "LicenseCreator" для создания лицензии через консоль`

![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20generator.gif)

`Или использовать WPF приложение`

![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/wpf_license_generator.png)

`для работы с файлом лицензии:`
```C#
  var controller = new SessionLicenseController(LicenseFilePath, Secret, StartNewSession, "Admin");
```
если лицензия истекла, или данные были скомпроментированы - на этом этапе вы получите ошибку

`чтобы посмотреть данные о лицензии:`
```C#
     controller.License.ToString(); //or controller.License.GetLicenseInformation()
```
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20info%20sample.png)

`посмотреть данные о сессиях`
```C#
    foreach (var (date_time, sessions) in controller.SessionController.GetSessionData())
    {
        $"Day: {date_time:dd.MM.yyyy}".ConsoleYellow();
        foreach (var session in sessions)
        {
            session.ConsoleRed();
        }
    }
```
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/license%20session%20sample.png)

`как проверить для консоли:`
```C#
var flag = true;
string row = null;
while (flag)
  try
  {
      var controller = row is null ? new SessionLicenseController(SessionsFilePath, Secret, true, "Admin") : new SessionLicenseController(row, Secret, SessionsFilePath, true, "Admin");
      flag = false;
      "License information:".ConsoleYellow();
      controller.License.ToString().ConsoleRed();
  }
  catch (Exception)
  {
      Console.WriteLine("License is bad, Enter license code or add file");
      row = Console.ReadLine();
  }
```

`как проверить для WPF:`
```C#
    LicenseChecker.CheckLicense("license.lic", "testwpf", true, "admin");
```
`если в лицензии на WPF будет ошибка - то интерфейс будет заблокирован окном ввода строки лицензии`
![Demo](https://github.com/Platonenkov/SessionLicenseControl/blob/dev/Resources/wpf_license_end.png)
