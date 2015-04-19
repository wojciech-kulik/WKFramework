# WKFramework  
[![Build status](https://ci.appveyor.com/api/projects/status/0gybmdy9wkxpw1nk/branch/development?svg=true)](https://ci.appveyor.com/project/Maxikq/wkframework/branch/development) [![Coverage Status](https://coveralls.io/repos/Maxikq/WKFramework/badge.svg?branch=development)](https://coveralls.io/r/Maxikq/WKFramework?branch=development) [![Nuget version](https://img.shields.io/nuget/v/WKFramework.Settings.svg)]() [![Nuget downloads](https://img.shields.io/nuget/dt/WKFramework.Settings.svg)]() [![License](https://img.shields.io/badge/license-MIT-blue.svg)]()

It's a small framework written in C#.  
Still **working on it**, so for now only settings, navigation and some utils are available.  

Any contributor is welcome.



## Settings
Key-value settings, where any object can be used as a key or value. For now there are two classes available, one to store settings in a file and another to store them in database:
- FileSettings
- MsSqlServerSettings

Both classes are configurable (you can pass your own serializer, set data type in database etc.). You can also easily attach MsSqlServerSettings to an existing database or create a new one using this class.

Writing to settings:
```c#
var person = new Person()
{
    FirstName = "John",
    LastName = "Smith"
};

var settings = FileSettings("settings.dat"); //the same interface is availalbe for MsSqlServerSettings

settings.WriteProperties(person); //saves only public properties
settings.WriteStaticProperties(typeof(Settings)); //you can also save properties from a static class
settings.WriteValue("LastUpdate", DateTime.Now);
settings.WriteValue(SettingsKey.Counter, 20);
```

Reading from settings:
```c#
var person = new Person();
var settings = FileSettings("settings.dat");

settings.ReadProperties(person);
settings.ReadStaticProperties(typeof(Settings));
var lastUpdate = settings.ReadValue<DateTime>("LastUpdate");
var counter = settings.ReadValue<int>(SettingsKey.Counter);
```

You can mark properties with **[NonSerializedProperty]** to avoid serializing them while using WriteProperties/WriteStaticProperties.



## Navigation (for WPF)
Service to help with navigation in ViewModel-First approach. It allows to show and setup window using a fluent interface.  
Windows are automatically localized using reflection, so the only thing you need to do is to stick to the naming convention: NameViewModel and NameView.

Example:
```c#
new NavigationService()
		.GetWindow<PersonDetailsViewModel>()
		.WithParam(vm => vm.Person, personToShow)
		.DoIfAccepted(vm => RefreshList())
		.ShowDialog();
```



## Utils
For now there are only two serializers: *BinarySerializer*, *GZipSerializer* and an extension which allows to perform a deep copy of an object (but it has to be marked as **[Serializable]**).