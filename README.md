# SalaryPlugin
Id: Feli.SalaryPlugin
Version: 1.0.2
Author: Feli
Website: fplugins.com

## Commands
- salary Correct command usage: /salary <RoleId> <on/off>: A command to set the status of a salary
  id: SalaryPlugin.Commands.SalaryCommand

## Permissions
- Feli.SalaryPlugin:commands.salary: Grants access to the SalaryPlugin.Commands.SalaryCommand command.

## Configuration
```
Salaries:
- RoleId: "unemployed"
  Payment: 5000
  Timer: 120
- RoleId: "vip"
  Payment: 6000
  Timer: 60
```

To add more salaries do:
```
Salaries:
- RoleId: "unemployed"
  Payment: 5000
  Timer: 120
- RoleId: "vip"
  Payment: 6000
  Timer: 60
- RoleId: "vip2"
  Payment: 62323
  Timer: 32
```

## Translations
```
SalaryPayment: "You have just recieved ${Money} for working as {RoleId}"
Usage: "Correct command usage: /salary <RoleId> <on/off>"
Finish: "Sucessfully set the salary {RoleId} to the status: {Status}"
```
