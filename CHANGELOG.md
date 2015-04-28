![](https://raw.github.com/kbilsted/StatePrinter/master/StatePrinter/gfx/stateprinter.png)

# Version history

Full documentation on usage and motivating examples at https://github.com/kbilsted/StatePrinter/tree/master/doc

## v2.1.207-rc

Added

  * Functionality for controlling automatic test rewrite using an environment variable.

  
## v2.1.198-rc

Added

  * Functionality for including or excluding fields and properties based on one or more type descriptions. See `IncludeByType()` and `ExcludeByType()`.

  
## v2.1.186-rc 

Fixed

  * [#22 Make error message configurable upon assertion failure](https://github.com/kbilsted/StatePrinter/issues/22)

Added

  * Added `AreAlike()`, replacing `IsSame()` (which is deprecated).
  * Made error message tell about `AreAlike()` when two strings are alike but not equals, when using `AreEquals()`.
  * Prepared for future expansion of functionality, by placing unit testing configuration in a sub-configuration class.

  
  
## v2.0.169

Added

* Added automatic unit test rewriting
* Added configuration of how line-endings are generated during state printing. This is to mitigate problems due to different operating systems uses different line-endings.
* Added assertion helper methods `Stateprinter.Assert.AreEqual`, `Stateprinter.Assert.IsSame`, `Stateprinter.Assert.PrintIsSame` and `Stateprinter.Assert.That`.  Improves the unit test experience by printing a suggested expected string as C# code.
* Added a `AllFieldsAndPropertiesHarvester` which is able to harvest properties and fields.
* `StringConverter` is now configurable with respect to quote character.
* BREAKING CHANGE: Projective harvester is now using the `AllFieldsAndPropertiesHarvester` rather instead of the `FieldHarvester`. This means both fields and properties are now harvested.


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
