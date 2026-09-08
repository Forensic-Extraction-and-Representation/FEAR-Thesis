# FEAR Framework

This repository contains the FEAR (Forensic Extraction and Representation) Framework—a comprehensive system for constructing semantic knowledge graphs from digital forensic artefacts. **This is a static archive of the codebase as it appears in the accompanying PhD thesis and is not actively maintained.**

## Overview

The FEAR Framework comprises:

- **GFEAR**: A declarative domain-specific language for mapping digital forensic artefacts to RDF triple representations
- **RFEAR**: A rule language for encoding expert knowledge and enabling inference over forensic knowledge graphs
- **Code-first ontology generation**: Automatic schema generation from GFEAR and RFEAR specifications
- **Neuro-symbolic AI integration**: Large language model grounding for query and interpretation using semantic knowledge graphs
- **Multiple ingestion interfaces**: Command-line tools, web interface, and Autopsy plugin integration

## Key Components

- `FEAR.Runtime`: C# implementation of the FEAR language runtime, entity identification, and knowledge graph operations
- `FEAR.WASM`: The .NET Blazor-based Web Interface for interacting with cases in the system
- `FEAR.Containers`: Python script to instantiate the Docker-containerised environment
- `FEAR.Lang`: ANTLR-based language processing for GFEAR and RFEAR transpilation
- Docker support: Jena Fuseki triple-store, PostgreSQL database, and Nginx reverse proxy configuration

## System Requirements

The reference setup uses:

- **Docker Desktop** (version 4.90.0 or later)
- **Windows Subsystem for Linux** (WSL)
- **Python** (version 3.14 or later)
- **Git** (version 2.34.1 or later)
- **Autopsy** (version 4.23.1 or later, for plugin integration)

For detailed setup instructions, see the accompanying thesis documentation.

## Quick Start

To run the FEAR Framework using Docker:

```bash
# Clone and navigate to the repository
git clone https://github.com/Forensic-Extraction-and-Representation/FEAR-Thesis.git
cd FEAR-Thesis

# Start the Docker environment
python FEAR.Containers/FEAR.Containers.py

# Update your local hosts file to include the following
127.0.0.1   api.fear.local
127.0.0.1   ui.fear.local
127.0.0.1   dbs.fear.local
127.0.0.1   scripts.fear.local

# Access the Web UI at https://api.fear.local/ and https://ui.fear.local/
```

## Related Projects

- [GFEAR-Autopsy](https://github.com/Forensic-Extraction-and-Representation/GFEAR-Autopsy-Thesis): Autopsy plugin module for the FEAR Framework

## Citation

This codebase is associated with the following PhD thesis:

> Korol, A. (2026). *A Framework for Mapping Artefacts to Knowledge Graph Representations for Digital Forensic Investigations*. PhD thesis, Edith Cowan University, Perth, Australia.

The repository represents the complete implementation of the FEAR Framework and supporting components described in the thesis, including the GFEAR and RFEAR domain-specific languages, the ontology generation pipeline, and the neuro-symbolic AI integration.

## Note

This repository is an archive snapshot matching the thesis evaluation