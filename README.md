# About this repo

## General info
This project is a "unofficial command-line interface" to the QRZ.COM website and require a personal account.
For more information about the website, go to: https://www.qrz.com  

## Requirements
* This program require Microsoft Windows EDGE Browser (pre-installed since Windows 8.1)

## Additional info
This release is a beta! It is therefore not considered "bug-free".
It's based on the automation of a web browser (web scraper), is still being optimized.

When you starting the software for the first time, it is advisable to view the list of available commands by typing "cm" ENTER
(You will find the list of available commands further down this page).

As you can see, commands consist of two characters and, as far as possible, are representations
memory of the functions they perform.

Some commands perform only one action, others need one or more parameters, still others return data.
The commands executed end up in the history (for each session) and to recover them, simply press
Up Arrow when the cursor is in the command box. It is possible to move the cursor back to the
command box by pressing the F2 key. With the DOWN arrow key it is possible to scroll the history forward.

ATTENTION! When using the Delete QSO ("dq") command: Before executing it, make sure that the position to be deleted
matches the selected one. The location of a QSO is determined by the ordering of the Logbook. Use
the "da" and "dd" commands to establish the order of the QSOs in the Logbook. (for security reasons, the "dq" command requires
entering 3 parameters, all the same, corresponding to the position to be deleted.

When adding a QSO to the Logbook using the "aq" command, the system automatically adds,
after the "aq" command SPACE ... the last QRZ in memory, the last frequency used and the last mode (SSB, AM, FM, etc...)
as well as the current date and time in UTC format. It is possible to move between the various parameters using the TAB key to scroll
right or SHIFT+TAB to scroll left.

Take a look at the list of all commands before starting.

Good Radio and excellent QSOs!


## Change Log
### 2023-01-06
* *First public commit

