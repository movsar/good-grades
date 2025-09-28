# Good Grades

**Good Grades** is an interactive educational platform for school students, featuring two applications:

- **GGManager** — tools for content administration and learning management  
- **GGPlayer** — a “player” for student assignments  

The project is designed for **offline-friendly learning on Windows**, **easy deployment**, and **regular updates**.

---

## Platform Components

### **GGManager (Admin Panel)**

- Create and manage lessons, modules, assignments, and materials  
- Organize assignments by courses and topics  
- Version control and publishing of updated materials  
- Built-in quality control tools (preview and quick edits)  
- Import/export support for backups or transferring content between installations  
- Application logs with automatic upload to a remote server for support and diagnostics  

### **GGPlayer (Student Application)**

- Access interactive assignments and course materials  
- Instant feedback on completion (correct/incorrect, hints)  
- View course resources including PDFs (WebView2 installation required for PDF support)  
- Clear and intuitive navigation through lessons and tasks  
- Automatic updates through releases  

### **SharedControls (Shared Library)**

- Common visual components and UI elements used by both GGManager and GGPlayer  
- Ensures a consistent look and behavior across applications  

---

## Key Features

### **Interactive Assignments**

- Tests, exercises, and step-by-step tasks  
- Highlighted results with hints for improvement  
- Ability to retry and reinforce learning  

### **Lesson Materials**

- Support for text and graphic resources  
- Built-in PDF support in the player  

### **Course and Section Management**

- Structure lessons by subjects, topics, and modules  
- Flexibly combine assignments and materials into lessons  

### **Interface Localization**

- Multi-language support via resource files (.resx)  
- Translate the interface without changing the application logic  

---

## Logs and Diagnostics

> Logging is powered by **Serilog** with a custom batching sink that sends logs to the [Good Grades Log Management API](https://github.com/movsar/good-grades.api). Delivery is guaranteed even when the app is offline.  

### Features of the Logging System

- 📦 **Buffered Server Delivery**  
  Logs are stored locally and periodically sent to:  
```

[https://ggapi.movsar.dev/Logs](https://ggapi.movsar.dev/Logs)

```
A buffer folder (`logs/outbox`) ensures no data loss while offline.  

- 📝 **Local Log Files**  
All events are also written to `logs/logs.txt` with daily rotation.  

- ⚙️ **Flexible Batching**  
Using `PeriodicBatchingSink`:  
- Up to **50 events** per batch  
- Sent every **5 seconds**  
- First event sent immediately  

- 🖥 **Enriched Log Data**  
Each event includes:  
- Machine name (`MachineName`)  
- OS version (`OSVersion`)  
- Application name and version  

- 🛡 **Minimum Log Level: Warning**  
Keeps production logs clean and noise-free.  

---

## Compatibility

- Runs on all Windows versions starting from **Windows 7**  
- May require **WebView2 installation** for PDF viewing  

---

## Who It’s For

- **Students** — a simple, intuitive “player” with instant feedback  
- **Teachers & Administrators** — tools to build lessons and courses, update materials, and perform basic quality control  

---

## Installation

1. **Download the installers** from the [Releases](../../releases) section  
 - Separate installers are available for **GGManager** and **GGPlayer**  
2. Run the installer and follow the setup wizard  

---

## Updates

- New versions are published in **Releases**  
- On startup, the application automatically checks for available updates and prompts the user  

---

## Screenshots

<p align="center">
<img src="https://github.com/user-attachments/assets/86159ba3-d629-4b3a-b404-98331041b095" width="24%" />
<img src="https://github.com/user-attachments/assets/11d916f7-de7f-4194-8a07-2dcd68a11563" width="23%" />
<img src="https://github.com/user-attachments/assets/731423e0-81f4-4b09-af7e-38fe9ffabcf9" width="22%" />
<img src="https://github.com/user-attachments/assets/817dc9d9-e45c-4624-aafa-5a602fd4af68" width="27%" />
</p>
