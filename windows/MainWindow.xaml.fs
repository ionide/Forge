namespace ViewModels

open System
open System.Windows
open ViewModule
open ViewModule.Validation
open FsXaml

type MainView = XAML<"MainWindow.xaml">

type MainViewModel() as self = 
    inherit ViewModelBase()    
