![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)

# Version history

## v. next version
* Configurable output as a single line.
* Improved unit test experience with the new `Stateprinter.Assert.AreEqual` and `Stateprinter.Assert.That`.
* Added a `AllFieldsAndPropertiesHarvester` which is able to harvest properties and fields.
* BREAKING CHANGE: Projective harvester is now using the `AllFieldsAndPropertiesHarvester` rather instead of the `FieldHarvester`.
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
