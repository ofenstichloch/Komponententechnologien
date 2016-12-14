# Komponententechnologien
## Contents 
Following folders in this Repo: 
**Rest-camel-RMI Adder**: An example program that offers a REST-Interface to add numbers.
  The Request is processed by a Camel instance that forwards the incoming REST-Request via RMI to another java application that adds these RMI-passed parameters.
  
**Team Solo Alex**: A C# and RabbitMQ based implementation of the project. Only the broker so far.

**Group**: Future folder for the team work.
## Protocol for communication between components
### Messages 
The components should communicate using a shared interface for messages. The Interface contains the following fields:

| Field | Values | Description |
| --- | --- | --- |
| int instruction |  {0=None, 1=Generated, 2=Solved, 3=Register, 4=Unregister} | Describes the reason for this message. A generator sends "generated"-messages, a solver "solved". |
| int type | { Broker, Generator, Solver, GUI} | Describes the type of component that is about to register/unregister, not used for non-registration messages |
| int[][] sudoku |  | Array for the Sudoku-field |
| string origin | URI | The URI of the component. This URI is where the foreign Camel-instance should connect to. |

Messages are, if needed, serialized to JSON. An example for this would be (to register a new generator):

```
{
  "info": 3,
  "instruction": 1,
  "sudoku": [
    [],
    [],
    []
  ],
  "origin": "rmi://localhost:1199/calc"
}
```
