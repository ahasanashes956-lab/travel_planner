#!/bin/bash
cd "$(dirname "$0")"
echo "🚀 Starting Travel Planner..."
echo "Opening http://localhost:8000..."
python -m http.server 8000 &
sleep 2
start http://localhost:8000
