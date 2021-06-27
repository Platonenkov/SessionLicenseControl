# SessionLicenseControl

Проект предназначен для учёта сессий, и контроля лицензии в ПО
При запуске считываются данные из зашифрованного архива и проверяются в зависимости от требований.

### Быстрый старт:

для работы с лицензией:
создание:

```C#
  var lic = new LicenseGenerator(Secret, hdd, expirationDate, Ouner, check_sessions);
  lic.CreateLicenseFile(LicenseFilePath);
```
где:

* `Secret` - secret string to encrypt data
* `hdd` - HDD id of PC where file will use
* `expirationDate` - Date when license will expire
* `Ouner` - for whom the license is created
* `check_sessions` - enable session control in licenses
* `LicenseFilePath` - path, where license file will saved

`You can use a console application "LicenseCreator" to create a license`


для работы с файлом лицензии:
```C#
  var controller = new SessionLicenseController(LicenseFilePath, Secret, StartNewSession, "Admin");
```
если лицензия истекла, или данные были скомпроментированы - на этом этапе вы получите ошибку

чтобы посмотреть данные о лицензии:
```C#
     controller.License.ToString(); //or controller.License.GetLicenseInformation()
```

посмотреть данные о сессиях
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
