# 🏆 SMPSO CPU Configuration Optimizer 🏆
A cross-platform .NET 8 application that implements the SMPSO genetic algorithm to explore the design space of processor microarchitectures using M-sim.
Pareto-optimal configurations output based on optimizing two key metrics: CPI (Cycles Per Instruction) and Power consumption.
<div align="center">
  <img src="https://raw.githubusercontent.com/R-a-d-u/SMPSO-CPU-Simulation/master/ParetoScreen.png" alt="Screen" width="325">
  <img src="https://raw.githubusercontent.com/R-a-d-u/SMPSO-CPU-Simulation/master/hashscreen.png" alt="Screen" width="675">
</div>

## ✨ Key Features
- Multi-objective optimization using SMPSO algorithm
- Real-time monitoring with interactive GTK-based GUI
- SQLite-based caching system to avoid redundant simulations
- Pareto front visualization with interactive charts
- CSV export functionality for post-processing analysis
- Cross-platform compatibility (Linux, Ubuntu and Fedora)

## 📋 Supported Metrics
- CPI (Cycles Per Instruction): Performance metric.
- Power Consumption: Average power per cycle in Watts. 

## 📊 Performance Considerations
### Hardware Recommendations:
- Multi-core CPU: Each simulation runs on a separate core.     
- Sufficient RAM: ~500MB per parallel simulation process.   

## 📚 References
This project builds upon several key research works:
- M-Sim: Multi-threaded architectural simulation environment
- SMPSO: Speed-constrained Multi-objective Particle Swarm Optimization
- Wattch: Framework for architectural-level power analysis
- SimpleScalar: Architectural simulation toolkit

## 🤝 Contributing
Contributions are welcome! Please feel free to submit issues, feature requests, or pull requests to improve the application.

## 📄 License
This project is developed for academic and research purposes. Please refer to the license file for usage terms and conditions.
