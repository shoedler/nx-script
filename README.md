# Nx Script

TODO

## Prerequisites (For Development)

- Java JDK 17 or 21
- ANTLR 4.13.1 [See Setup Guide](#antlr4-toolchain---windows-11-setup-quickguide)

## Usage

Build the project.

```shell
dotnet build
```

Run the sample script.

```shell
.\NxScript\bin\Debug\net7.0\nxs.exe run .\NxScript\SampleScript.nx
```

# ANTLR4 Toolchain - Windows 11 Setup Quickguide

## Install JDK

1. Download & Install JDK (Java Devkit) Version 17 or 21
   https://www.oracle.com/java/technologies/downloads/

2. Open your **system** environment variables. Open the `Path` variable and add a link to the JDK _bin_ directory. (Usually `C:/Program Files/Java/jdk/bin`)

> If you already have a Java installation, move the new Entry in `Path` up, so that it's higher priority.

## Install ANTLR4

1. Download the ANTLR4 jar. Currently Version 4.13.1 (Nov 2023) from
   https://www.antlr.org/download.html (klick on the **"ANTLR tool itself"** link)

2. Move the downloaded jar to `C:/Program Files/Java/libs` (make a new directory if it does not exist)

3. In the **system** environment variables, add a new variable called `CLASSPATH`. Set the Value to `C:/Program Files/Java/libs/antlr-4.13.1-complete.jar`

## Setup ANTLR Batch Files

We'll create two convenience batch files which help interacting with ANTLR4. `grun` will help you test your grammars with the ANTLR4 built-in test rig. `antlr4` offers a shortcut to the ANTLR4 tool cli.

1. In `C:/Program Files/Java/libs/` create a directory called "batches". Make two new batches: `antlr4.bat` & `grun.bat`
   (You probably have to create the files somwhere else because of the directory protection).

### `grun.bat`

```batch
@echo off
set TEST_CURRENT_DIR=%CLASSPATH:.;=%
if "%TEST_CURRENT_DIR%" == "%CLASSPATH%" ( set CLASSPATH=.;%CLASSPATH% )
@echo on
java org.antlr.v4.gui.TestRig %*
```

### `antlr4.bat`

```batch
java org.antlr.v4.Tool %*
```

3. Open your **user** environment variables. Open the `Path` variable, and add a link to the _batches_ directory. E.g. `C:/Program Files/Java/libs/batches`

4. Test by opening a terminal and call `antlr4` and `grun`

## `grun` Commands

Example grammar name: _MyGrammar_, Example Starting Rule: _program_

- Basic call: `grun MyGrammar program`
- List tokens: `grun MyGrammar program -tokens`
- CLI tree: `grun MyGrammar program -tree`
- GUI tree: `grun MyGrammar program -gui`
- Load from File: `grun MyGrammar program testCode.mygrammar`
