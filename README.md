# 📘 Budget Buddy – Personal Finance Tracker

A lightweight .NET console app for importing, managing, querying, and exporting personal finance transactions.

---

## 🚀 Features

### **Import**
- `import <file1.csv> [file2.csv ...]`
- Parallel CSV import, async I/O
- Skips duplicate IDs, logs malformed rows
- Supports cancellation (Ctrl + C)

### **List & Query**
- `list all`
- `list month <yyyy-MM>`
- `by category <name>`
- `over <amount>`
- `search <text>`

### **Edit**
- `set category <id> <name>`
- `rename category <old> <new>`
- `remove <id>`

### **Statistics**
- `stats month <yyyy-MM>`
- `stats yearly <yyyy>`

### **Export**
- `export json <path>`
- `export csv <path>`
- Overwrite prompt (y/n), async export

---

## 🧪 Unit Tests
Covers:
- TransactionFactory  
- Repository  
- StatsService  
- Extensions  
(All tests passing)

---

## 📝 Example Commands

```text
import data/jan.csv data/feb.csv
list month 2025-01
search shop
set category 2 Groceries
stats yearly 2025
export json out.json
