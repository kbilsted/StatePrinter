![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)

# Version history


## 2.0.105-rc - 2.0.151-rc
* Added automatic unit test rewriting
* Added configuration of how line-endings are generated during state printing. This is to mitigate problems due to different operating systems uses different line-endings.
* Added assertion helper methods `Stateprinter.Assert.AreEqual`, `Stateprinter.Assert.IsSame`, `Stateprinter.Assert.PrintIsSame` and `Stateprinter.Assert.That`.  Improves the unit test experience by printing a suggested expected string as C# code.
* Added a `AllFieldsAndPropertiesHarvester` which is able to harvest properties and fields.
* `StringConverter` is now configurable with respect to quote character.
* BREAKING CHANGE: Projective harvester is now using the `AllFieldsAndPropertiesHarvester` rather instead of the `FieldHarvester`. This means both fields and properties are now harvested.
* BREAKING CHANGE: The deprecated type `StatePrinter` is now deleted. Use `Stateprinter` instead.


## v1.0.6

Added

* Executing stylecop on the build server.
* Made the `Configuration` class API a bit more fluent
* BUGFIX: Harvesting of types were cached across `Stateprinter` instances, which no longer makes sense since harvesting is configurable from instance to instance.
* BUGFIX: Changed how `ToString()` methods are harvested. Thanks to "Sjdirect".


## v1.0.5

Added

* Support for using the native `ToString()` implementation on types through a field harvester
* Added a Projective field harvester to easily reduce the harvesting of selective fields on types in a type-safe manner. See the section on unit testing in the readme.md
* Added the type `Stateprinter` and obsoleted the `StatePrinter` type


## v1.0.4


Added

* CLS compliance
* 20% Performance boost



Have fun!

Kasper B. Graversen
