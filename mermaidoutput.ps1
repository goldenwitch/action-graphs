# Build mermaid files for each sample

# Ensure dependencies: dotnet 3.1, npm, mermaid cli

# First, build an exe for our CLI project
dotnet build actiongraph-cli\actiongraph-cli.fsproj

# Second, copy all of the graph files to our bin
copy ClockSample\clockGraph.json actiongraph-cli\bin\Debug\netcoreapp3.1
copy RpgSample\textAdventure.json actiongraph-cli\bin\Debug\netcoreapp3.1
copy EchoGraph\echoGraph.json actiongraph-cli\bin\Debug\netcoreapp3.1

# Third, call ag-cli to mermaid all of the sample graphs
cd actiongraph-cli\bin\Debug\netcoreapp3.1
.\actiongraph-cli mermaid clockGraph.json clockGraph.txt
.\actiongraph-cli mermaid textAdventure.json textAdventure.txt
.\actiongraph-cli mermaid echoGraph.json echoGraph.txt
# Fourth, call mermaid-cli to svg all of the mermaid files
npm install @mermaid-js/mermaid-cli
npx mmdc -i clockGraph.txt -o clockGraph.svg
npx mmdc -i textAdventure.txt -o textAdventure.svg
npx mmdc -i echoGraph.txt -o echoGraph.svg

# Fifth, manually do things to get these into github XD
cd ..\..\..\..
copy actiongraph-cli\bin\Debug\netcoreapp3.1\clockGraph.svg ClockSample\
copy actiongraph-cli\bin\Debug\netcoreapp3.1\textAdventure.svg RpgSample\
copy actiongraph-cli\bin\Debug\netcoreapp3.1\echoGraph.svg EchoGraph\