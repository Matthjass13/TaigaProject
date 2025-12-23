pipeline {
  agent any

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Restore') {
      steps { bat 'dotnet restore' }
    }

    stage('Build') {
      steps { bat 'dotnet build -c Release --no-restore' }
    }

    stage('Test') {
      steps { bat 'dotnet test -c Release --no-build' }
    }
  }
}
