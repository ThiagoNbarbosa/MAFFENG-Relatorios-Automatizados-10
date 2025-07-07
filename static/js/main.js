// Main JavaScript functionality
document.addEventListener('DOMContentLoaded', function() {
    // File upload handling
    const fileInput = document.getElementById('arquivo_zip');
    const fileName = document.getElementById('file-name');
    const form = document.getElementById('uploadForm');
    const submitBtn = document.getElementById('submitBtn');
    const btnText = document.getElementById('btn-text');
    const progressContainer = document.getElementById('progress-container');
    const progressBar = document.getElementById('progress-bar');
    const progressText = document.getElementById('progress-text');
    
    // Handle file selection
    fileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            if (file.name.toLowerCase().endsWith('.zip')) {
                fileName.textContent = file.name;
                fileName.parentElement.classList.add('border-green-500');
                fileName.parentElement.classList.remove('border-red-500');
                
                // Check file size (100MB limit)
                const maxSize = 100 * 1024 * 1024;
                if (file.size > maxSize) {
                    showNotification('Arquivo muito grande. Tamanho máximo: 100MB', 'error');
                    fileInput.value = '';
                    fileName.textContent = 'Selecionar arquivo ZIP';
                    fileName.parentElement.classList.remove('border-green-500');
                    return;
                }
                
                showNotification('Arquivo ZIP selecionado com sucesso!', 'success');
            } else {
                fileName.textContent = 'Apenas arquivos ZIP são permitidos';
                fileName.parentElement.classList.add('border-red-500');
                fileName.parentElement.classList.remove('border-green-500');
                fileInput.value = '';
                showNotification('Apenas arquivos ZIP são permitidos', 'error');
            }
        } else {
            fileName.textContent = 'Selecionar arquivo ZIP';
            fileName.parentElement.classList.remove('border-green-500', 'border-red-500');
        }
    });
    
    // Form validation
    form.addEventListener('submit', function(e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }
        
        // Show loading state
        showLoadingState();
        
        // Simulate progress (since we can't track actual server progress)
        simulateProgress();
    });
    
    // Form validation function
    function validateForm() {
        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;
        
        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                isValid = false;
                field.classList.add('border-red-500');
                field.classList.remove('border-green-500');
            } else {
                field.classList.add('border-green-500');
                field.classList.remove('border-red-500');
            }
        });
        
        if (!isValid) {
            showNotification('Por favor, preencha todos os campos obrigatórios', 'error');
            
            // Focus on first invalid field
            const firstInvalid = form.querySelector('.border-red-500');
            if (firstInvalid) {
                firstInvalid.focus();
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }
        
        return isValid;
    }
    
    // Show loading state
    function showLoadingState() {
        submitBtn.disabled = true;
        submitBtn.classList.add('loading');
        btnText.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Processando...';
        progressContainer.classList.remove('hidden');
    }
    
    // Simulate progress
    function simulateProgress() {
        let progress = 0;
        const interval = setInterval(() => {
            progress += Math.random() * 15;
            if (progress > 90) {
                progress = 90;
            }
            
            progressBar.style.width = progress + '%';
            progressText.textContent = Math.round(progress) + '%';
            
            if (progress >= 90) {
                clearInterval(interval);
                progressText.textContent = 'Finalizando...';
            }
        }, 500);
    }
    
    // Show notification
    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `fixed top-4 right-4 z-50 max-w-sm p-4 rounded-lg shadow-lg transform transition-all duration-300 translate-x-full ${
            type === 'success' ? 'bg-green-600' : 
            type === 'error' ? 'bg-red-600' : 
            'bg-blue-600'
        }`;
        
        notification.innerHTML = `
            <div class="flex items-center">
                <i class="fas ${
                    type === 'success' ? 'fa-check-circle' : 
                    type === 'error' ? 'fa-exclamation-circle' : 
                    'fa-info-circle'
                } mr-3"></i>
                <span class="text-white">${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" class="ml-auto text-white hover:text-gray-300">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => {
            notification.classList.remove('translate-x-full');
        }, 100);
        
        // Auto remove after 5 seconds
        setTimeout(() => {
            notification.classList.add('translate-x-full');
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, 300);
        }, 5000);
    }
    
    // Real-time form validation
    const inputs = form.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.hasAttribute('required')) {
                if (!this.value.trim()) {
                    this.classList.add('border-red-500');
                    this.classList.remove('border-green-500');
                } else {
                    this.classList.add('border-green-500');
                    this.classList.remove('border-red-500');
                }
            }
        });
        
        input.addEventListener('input', function() {
            if (this.classList.contains('border-red-500') && this.value.trim()) {
                this.classList.add('border-green-500');
                this.classList.remove('border-red-500');
            }
        });
    });
    
    // Auto-set current date for data_elaboracao
    const dataElaboracao = document.querySelector('input[name="data_elaboracao"]');
    if (dataElaboracao && !dataElaboracao.value) {
        const today = new Date();
        const formattedDate = today.toISOString().split('T')[0];
        dataElaboracao.value = formattedDate;
    }
    
    // Smooth scrolling for form sections
    const formSections = document.querySelectorAll('.glass-card');
    formSections.forEach((section, index) => {
        section.style.animationDelay = `${index * 0.1}s`;
        section.classList.add('animate-fade-in');
    });
});

// Drag and drop functionality for file upload
function initDragAndDrop() {
    const fileInput = document.getElementById('arquivo_zip');
    const dropZone = fileInput.parentElement;
    
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
    });
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, highlight, false);
    });
    
    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, unhighlight, false);
    });
    
    function highlight(e) {
        dropZone.classList.add('border-blue-500', 'bg-blue-500/20');
    }
    
    function unhighlight(e) {
        dropZone.classList.remove('border-blue-500', 'bg-blue-500/20');
    }
    
    dropZone.addEventListener('drop', handleDrop, false);
    
    function handleDrop(e) {
        const dt = e.dataTransfer;
        const files = dt.files;
        
        if (files.length > 0) {
            fileInput.files = files;
            fileInput.dispatchEvent(new Event('change'));
        }
    }
}

// Initialize drag and drop when page loads
document.addEventListener('DOMContentLoaded', initDragAndDrop);
