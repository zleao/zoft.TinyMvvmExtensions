[![Build status](https://zleaopereira.visualstudio.com/CrossDevelopment/_apis/build/status/zoft.TinyMvvmExtensions)](https://zleaopereira.visualstudio.com/CrossDevelopment/_build/latest?definitionId=4)

Nuget Package | Current Version
--- | ---
| zoft.TinyMvvmExtensions | [![NuGet](https://img.shields.io/nuget/v/zoft.TinyMvvmExtensions.svg)](https://www.nuget.org/packages/zoft.TinyMvvmExtensions/)
| zoft.TinyMvvmExtensions.Core | [![NuGet](https://img.shields.io/nuget/v/zoft.TinyMvvmExtensions.Core.svg)](https://www.nuget.org/packages/zoft.TinyMvvmExtensions.Core/)
| zoft.TinyMvvmExtensions.Forms | [![NuGet](https://img.shields.io/nuget/v/zoft.TinyMvvmExtensions.Forms.svg)](https://www.nuget.org/packages/zoft.TinyMvvmExtensions.Forms/)


# zoft.TinyMvvmExtensions

Extension library built on top of the awesome TinyMvvm library. Similar approach to what I've done in the [MvxExtensions](https://github.com/zleao/MvvmCross-Extensions/tree/master/Libraries/MvxExtensions) library. Provides extra functionalites for ViewModels.

### A bit of context:

This project is part of the **zoft** library, which is nothing more than an attempt to put together a set of C# utilities that I've created in my current and past projects.

A lot of this code comes from another library I've developed over the years ([MvxExtensions](https://github.com/zleao/MvvmCross-Extensions)). This library was initialy built to provide some extensions and utilities that work on top of awesome [MvvmCross](https://github.com/MvvmCross/MvvmCross)... but then it came a time to change and here we are. **zoft** is my attempt to extend the awesome [TinyMvvm](https://github.com/TinyStuff/TinyMvvm) library in the same way I did for MvvmCross. It starts with code ported from MvxExtentions but it will not stop there...

##

# Get Started

## Installing Nuget Packages
Depending on the desired functionalities, there are different packages to install. Please refer to the [Scope of each nuget package](#Scope_of_each_nuget_package) for more info.

## Don't forget to register the services in the IO Container
Irregardless of the IO Contianer you're using, it's important to register the services you might want to use:

- zoft.NotificationService - the INotificationsService must/should be registered as singleton
`containerBuilder.RegisterType<NotificationManager>().As<INotificationService>().SingleInstance();`
- The ILocalizationService must/should be registered as singleton
`containerBuilder.RegisterInstance<ILocalizationService>(new ResourceManagerLocalizationService(AppResource.ResourceManager, CultureInfo.InvariantCulture));`

##

# Scope of each nuget package

|| zoft.TinyMvvmExtensions.Core | zoft.TinyMvvmExtensions | zoft.TinyMvvmExtensions.Forms
--- | --- | ---| ---
**Features** | <ul><li>Base ViewModel functionalities including:<br><ul><li>Inheritance from TinyMvvm.ViewModelBase</li><li>*DependsOn* logic</li><li>*IsBusy* execution logic</li></ul></li><li>Support for Event Weak Subscription</li><li>Support for *Validatable Objects*</li><li>Localization support via service</li><li>Sync and Async Command</li><li>Several extensions for ease of use</li></ul> | <ul><li>All included in zoft.TinyMvvmExtensions.Core</li><li>Support for Localization</li><li>Base support for sub/pub like notifications (via [zoft.NotificationService](https://github.com/zleao/zoft.NotificationService))</li></ul> | <ul><li>Base support for the sub/pub notifications present in *zoft.TinyMvvmExtenions*</li><li>XAML extension for Localization</li><li>Base support for the validatable objects</li></ul> |
**Dependencies**<br>(for the most recent version) |<ul><li>.NetStandard2.0</li><li>TinnyMvvm v3.0.1-pre4+</li><li>Xamarin Essentials v1.6.0+</li></ul> | <ul><li>.NetStandard2.0</li><li>TinnyMvvm v3.0.1-pre4+</li><li>Xamarin Essentials v1.6.0+</li><li>zoft.TinyMvvmExtensions.Core v1.0.0-beta6+</li><li>zoft.NotificationService v1.0.0+</li></ul> | <ul><li>.NetStandard2.0</li><li>TinnyMvvm.Forms v3.0.1-pre4+</li><li>zoft.TinyMvvmExtensions v1.0.0-beta6+</li><li>zoft.NotificationService v1.0.0+</li><li>zoft.TinyMvvmExtensions v1.0.0-beta6+</li><li>Xamarin.Forms v4.8.0.1821</li></ul>
**When and Where to use** | **When**<br>Use when you don't need the extra functionality of the *zoft.TinyMvvmExtensions*.<br><br>**Where**<br>Place it where your ViewModels are. | **When**<br>To use when you want to have the base boilerplte code, to have support for extra the functionalities of this package. <br><br>**Where**<br>Place it where your ViewModels are. | **When**<br>Use when you're also using the *zoft.TinyMvvmEtensions* package. It provides out-of-the-box connection with the functionalities of that package. <br><br>**Where**<br>Place it in the Xamarin.Forms related projects (Shared and Platform specific ones). 


# Samples
Check out the [sample project](https://github.com/zleao/zoft.TinyMvvmExtensions/tree/main/src/Samples/ClassicForms). Also, don't forget to take a look at [TinyMvvm](https://github.com/TinyStuff/TinyMvvm) for more info on how to get started with TinyMvvm.

