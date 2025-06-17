### BattlEye is a client-server anti-cheat consisting of a client library (BEClient_x64.dll ) and the server library (BEServer_x64.dll ). The anti-cheat works on the principle of a black box. At the initialization stage, we send pointers to functions and instead receive an internal API for working with it.
## Services and drivers involved in the operation
| DLL/Service/Driver | Description |
| --- | --- |
| BEClient | connection to BEService, connection to BEServer |
| BEServer | kick,ban,cheat check requests, text messages |
| BEService | monitoring the loading of the anticheat kernel driver, BEClient <-BE Service-> BEDaisy |
| BEDaisy | anticheat kernel driver |
