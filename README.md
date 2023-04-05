## Creating a Programming Language

### 2.1. Design Stages

Designing a programming language involves making design decisions regarding grammar, architecture, and implementing software modules in a specific sequence.

**Syntactic Constructs**: Design of all basic concepts of the new language. It is advisable to use languages with static data typing for system or tool programming.

**Lexer**: Performs lexical analysis, converting a sequence of characters into a sequence of lexemes.

**Parser**: Takes input data and creates a data structure, such as an abstract syntax tree or another hierarchical structure.

### 2.2 Code Generation

There are two main types of code generation:

1. **Interpretation**: An interpreter program performs per-statement processing, conversion to machine code, and execution of a program or query.
2. **Compilation**: A compiler translates computer code written in one programming language into another language, mainly used for translating high-level programming language source code to lower-level languages.

These are the four main components that form a functional, cohesive system. Additional modules include a code editor, debugger, and optimizations.

## 4. Creating Algorithms and Software Modules

## 4.1 General Project Provisions

The main idea of the project: A character stream is input to the program (e.g., a `.alan` file) containing a sequence of executable commands. 

The process involves:

1. **Lexer**: Performs lexical analysis to extract lexemes from the input stream.
2. **Parser**: Filters lexemes and performs syntactic and semantic analysis, producing an abstract syntax tree.
3. **Compiler**: Translates the abstract syntax tree into JVM Code and uses the Jasmin module to execute the command sequence on the JVM.

## 4.2 Alan Language Syntactic Constructs

The Alan language has 11 syntactic entities:

- Keywords: while, if, then, else, do, for, upto, true, false, read, write, skip
- Operators: +, -, *, %, /, ==, !=, >, <, >=, <=, :=, &&, ||
- Letters: upper and lowercase
- Symbols: letters and . _> < = ; ,  :
- String literals: enclosed in "..." and contain characters, spaces, digits
- Brackets: (, {, ) and }
- Semicolon: ;
- Spaces: " " (one or more), \n, \t
- Identifiers: letters, followed by _, letters, or digits
- Numbers: 0, 1, ... etc.; numbers do NOT start with zero: 001
- Comments: start with // and contain characters, spaces, and digits to the end of the line

### 4.3 Lexer

The Alan language lexer sequentially divides the code into lexemes (strings with assigned and thus determined values), as shown in Appendix B. The lexer code is written in C# following clean code principles [9]. The lexer code is presented in Appendix G. The main provisions of the lexer are the rules for dividing syntactic constructs into lexemes.

Next, all spaces and comments are programmatically removed from the list of lexemes. The resulting data is passed to the parser.

### 4.4 Parser

The parsing technique used is Parser Combinators. Their most notable feature is that they are easy to implement (especially when using a functional programming language). Another advantage of parser combinators is that they can deal with any input data as long as it has a "sequence," such as a string or a list of lexemes [6]. The only two properties of input data needed are the ability to check when it's empty and to "sequentially" parse it. Strings and lists meet this requirement. However, parser combinators have their drawbacks, such as requiring the syntax analysis of the grammar to be non-left-recursive and being effective only when the grammar is unambiguous. The grammar designer's responsibility is to maintain these two properties. The general idea of parser combinators is to transform input data into sets of pairs.

For analyzing different types of syntactic structures, the following combinations are used:
- Parser1 ~ Parser2 - alternative parser
- Parser1 | Parser2 - sequential parser
- Parser1.map(function) - semantic action parser

From now on, for building the abstract syntax tree, we introduce the following grammatical structures:
- Arithmetic expressions
- Logical expressions
- Individual expressions
- Compound expressions separated by a semicolon
- Blocks surrounded by curly braces {..}

Note that the grammar is right-recursive.

### 4.5 Interpreter

During the project, the author implemented an interpreter and compiler for the Alan language for further comparison. Therefore, in this section, we will discuss the advantages and disadvantages of language interpretability.

The main advantage of the interpreter is the ease of writing it. The Alan language interpreter is only 40 lines of code, written in Scala and presented in Appendix E. However, due to their high-level nature, interpreters tend to be slow in executing programs with a large number of operations. More details on execution time in section 4.7. Interpreted language programs are executed every time they run and require an available interpreter.

### 4.6 Compiler

The Alan language compiler consists of three main parts:
- A program that forms a list of commands in the target executable language (JVM Code) from the abstract syntax tree. See the code of this component in Appendix J.
- The Jasmin module that executes the received commands on the virtual machine (JVM) [14].
- The built-in IntelliJ IDEA decompiler that forms convenient and understandable Java code.

Compilers differ in their complexity of writing, the presence of many pitfalls, and the need for optimizations for more efficient operation. The compiler creates an executable program (.exe), making running programs simpler and faster.

### 4.7 Comparison of interpretation and compilation

During the project, an interpreter and compiler for the Alan language were created, so we will choose the optimal translator option based on experimental research.

During the experiment, a series of test measurements (Figure 4.6) were carried out on the execution time of the program (Appendix H). Basic language libraries were used to measure the time characteristics. For Alan, a chronograph was used.

Let's compare the work of the C# and Alan language compilers. Table 4.1 shows the results of time characteristics measurements (in seconds) relative to the complexity of the input data stream. A visual comparison of the work of these compilers is presented in Figure 4.7.

Experimental time compilation indicators of the program in Appendix H
in Alan and C# languages

|   | Alan | C# |
|---|------|----|
| start = 100 | 3.3 | 1 |
| start = 500 | 3.9 | 2.9 |
| start = 1000 | 4.1 | 20.5 |

Thus, the Alan language compiler outperforms its C# counterpart with a large number of operations, as shown in Figure 4.7.

Similarly, let's compare the Alan and Python language interpreters. Execution time measurements are shown in Table 4.2, and graphical comparison in Figure 4.8.

We observe a similar dependency: the Alan interpreter outperforms its Python counterpart.

Experimental time interpretation indicators of the program in Appendix H
in Alan and Python languages

|   | Alan | Python |
|---|------|--------|
| start = 100 | 3 | 0.4 |
| start = 500 | 17 | 35.9 |
| start = 1000 | 115 | 348.5 |

We have proven the effectiveness of the Alan language interpreter and compiler. By experimentally comparing the execution time of the test program for both translators (Figure 4.9), a significant (about 1000 times) performance advantage of the Alan compiler is revealed; it is accepted as the basis for code generation of the project.

### CONCLUSIONS

The creation of software design tools is becoming increasingly relevant. These tools need to be the most intelligent and technologically advanced. In this work, a wide range of programming languages and environments, existing software design tools, were studied and analyzed. A comparative analysis was conducted, optimal tools for project implementation were selected, an alternative custom language and programming system were created, and the efficiency of the obtained software product was evaluated.

The "Alan" project is relevant and important since the research and practical developments are based on modern programming paradigms. The work has a theoretical-applied nature. The author created and described the lexical analysis algorithm, the syntax analysis algorithm of the input stream of lexemes, and the algorithm for interpreting the abstract syntax tree into executable machine code. The obtained qualitative and quantitative characteristics confirm the effectiveness of the developed software application for designing specialized software using the Alan language.

The project is multi-module and anticipates further development. At this stage, the Alan language program code compiler provides efficient processing of numerical and textual data.
