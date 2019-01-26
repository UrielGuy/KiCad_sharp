# KiCad_sharp
A C# library wrapping some of KiCad's functionallity, allowing for some features not in the software and programmatic circuit creation.
It was created for a specific project and since has proven useful for many of them. For simple boards I rather work with this as it allows for easier changes.
## Why use this library?
This library is REALLY usefull in a few cases: 
- Automating repeptative tasks - such as locating LEDs or buttons in a pattern.
- If you need to create a board with a parametric design.
- Creating PCBs with rounded features. While arcs are much better with KiCad 5, round traces are still not supported natively. This library allows for rounded traces quite easily
- If you want to easily export information about the circuit to other programs (so far I only developed support for OpenScad)
- The automatic programming jig generation is quite useful.

## why NOT use this library?
- If you have a very complicated board with lots of component then coding them will be tedious
- If you need more than two layers (because I've only implemented two layers)

## Usage Guidelines:
1. Clone the repository locally
2. Open the solution and compile the KiCad_sharp project
3. Create you own project, and add a reference to the compiled (or project library)
4. Create a new KiCad.PCB() object. 
At this stage I'd recommend checking the samples in the solution. They show alot of different uses of the library.
5. Manipulate the PCB as much as you want.
6. Use System.IO.File.WriteAllText("file_path.kicad_pcb", pcb.ToString()) to write out the PCB.
7. Open the file in PCBNEW
8. Make sure you're happy with the board generated. 
9. Press 'B' to fill the zones. 
10. run the DRC checks, just to make sure. Also pree "check unconnected" 
11. You're done. Plot out your board and have it made at your favorite PCB fab. 

## Known Issues: 
- Text tags are not rotated with components
- API for arcs is not uniform between drawing layers and Copper layers
- Bounds are not properly calculated for arcs.

I recommend the checking the wiki for explenation and common use cases for elements of the Library
