Feature: Examples
	In order to test and demo DataDriver functionalities

Scenario: test data driven
	When fill in following data to "TestObj":
	| Field      | Value                           |
	| name       | ryan                            |
	| stringList | ["5","6","7","8"]               |
	| boolList   | [true,false]                    |
	| intList    | [5,6,7,8]                       |
	| nest       | {name:"song"}                   |
	| nestList   | [{name:"song"}, {name:"song2"}] |
	| ignore     | n/a                            |
	Then verify the following data in "TestObj" should pass:
		| Field                | Value                        |
		| name                 | ryan                         |
		| stringList           | ["5","6","7","8"]            |
		| boolList             | [true,false]                 |
		| intList              | [5,6,7,8]                    |
		| nest                 | {name:"song"}                |
		| nestList             | [{name:"song"}, {name:"song2"}] |
		| ignore               | n/a                          |
		| nest                 | {name:n/a}                   |
		| stringList.Contain() | 5                            |
		| nestList.Count()     | 2                            |
		| nestList.Contain()   | {name:"song"}                |
    	Then verify the following data in "TestObj" should fail:
		| Field | Value |
		| name  | ryan1 |
		Then verify the following data in "TestObj" should fail:
		| Field   | Value |
		| invalid | ryan  |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| stringList | ["5","6","7"] |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| stringList | ["5","6","7"] |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| boolList   | [true,true]                    |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| intList    | [5,6,7,8,9]                    |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| nest       | {name:"song1"}                 |
		Then verify the following data in "TestObj" should fail:
		| Field      | Value         |
		| nestList   | [{name:"song"}, {name:"song"}] |

