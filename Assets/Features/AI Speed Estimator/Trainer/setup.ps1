# Check if Python is installed
$python = Get-Command python3 -ErrorAction SilentlyContinue
if (-not $python) {
    $python = Get-Command python -ErrorAction SilentlyContinue
    if (-not $python) {
        Write-Host "Python is not installed. Please install Python to continue." -ForegroundColor Red
        exit 1
    }
}

# Create virtual environment if it doesn't exist
if (-not (Test-Path -Path ".venv")) {
    Write-Host "Creating virtual environment..." -ForegroundColor Green
    python -m venv .venv
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Python venv creation failed!" -ForegroundColor Red
        exit 1
    }
}

# Activate the virtual environment
$activateScript = ".\.venv\Scripts\Activate.ps1"

if (-not (Test-Path -Path $activateScript)) {
    Write-Host "Virtual environment activation script not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Activating virtual environment..." -ForegroundColor Green
& $activateScript

# Check if activation was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to activate virtual environment!" -ForegroundColor Red
    exit 1
}

# Install dependencies from requirements.txt if it exists
if (Test-Path -Path "requirements.txt") {
    Write-Host "Installing dependencies from requirements.txt..." -ForegroundColor Green
    pip install -r requirements.txt
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install dependencies!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "No requirements.txt found, skipping dependency installation." -ForegroundColor Yellow
}

Write-Host "Setup complete. To activate the virtual environment manually, run:" -ForegroundColor Green
Write-Host "    .\.venv\Scripts\Activate.ps1" -ForegroundColor Green
