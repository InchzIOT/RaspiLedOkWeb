// Configuration Page JavaScript Functions

function testConnection() {
    const endpoint = document.querySelector('input[name="ApiUrl"]').value;
    const username = document.querySelector('input[name="Username"]').value;
    
    if (!endpoint || !username) {
        showAlert('Please enter both endpoint and username before testing.', 'warning');
        return;
    }
    
    fetch('Configuration/TestConnection', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert(data.message, 'success');
        } else {
            showAlert(data.message, 'danger');
        }
    })
    .catch(error => {
        console.error('Error testing connection:', error);
        showAlert('Error testing connection.', 'danger');
    });
}

function syncAssets() {
    const syncBtn = document.getElementById('syncBtn');
    const syncResult = document.getElementById('syncResult');
    
    // Disable button and show loading
    syncBtn.disabled = true;
    syncBtn.innerHTML = '<i class="bi bi-arrow-clockwise spin"></i> Syncing...';
    syncResult.innerHTML = '<div class="alert alert-info"><i class="bi bi-info-circle"></i> Syncing assets from API...</div>';
    
    fetch('Configuration/SyncAssets', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            syncResult.innerHTML = `<div class="alert alert-success"><i class="bi bi-check-circle"></i> ${data.message}</div>`;
            // Load assets after successful sync
            loadAssets();
        } else {
            syncResult.innerHTML = `<div class="alert alert-danger"><i class="bi bi-exclamation-triangle"></i> ${data.message}</div>`;
        }
    })
    .catch(error => {
        console.error('Error syncing assets:', error);
        syncResult.innerHTML = '<div class="alert alert-danger"><i class="bi bi-exclamation-triangle"></i> Error syncing assets.</div>';
    })
    .finally(() => {
        // Re-enable button
        syncBtn.disabled = false;
        syncBtn.innerHTML = '<i class="bi bi-arrow-clockwise"></i> Sync Assets';
    });
}

function loadAssets() {
    const container = document.getElementById('assetsContainer');
    container.innerHTML = '<div class="text-center"><i class="bi bi-arrow-clockwise spin"></i> Loading assets...</div>';
    
    fetch('Configuration/GetAssets')
        .then(response => response.json())
        .then(data => {
            if (data.success && data.assets && data.assets.length > 0) {
                renderAssets(data.assets);
            } else {
                container.innerHTML = `
                    <div class="text-center text-muted py-4">
                        <i class="bi bi-box-seam display-4"></i>
                        <p class="mt-2">No assets found. Click "Sync Assets" to load from API.</p>
                    </div>`;
            }
        })
        .catch(error => {
            console.error('Error loading assets:', error);
            container.innerHTML = '<div class="alert alert-danger">Error loading assets.</div>';
        });
}

function renderAssets(assets) {
    const container = document.getElementById('assetsContainer');
    
    if (!assets || assets.length === 0) {
        container.innerHTML = `
            <div class="text-center text-muted py-5">
                <i class="bi bi-collection display-1 text-muted mb-3"></i>
                <h5 class="text-muted">No Assets Found</h5>
                <p class="mb-0">Click "Sync Assets" to load assets and devices from your API</p>
            </div>`;
        return;
    }
    
    let html = '<div class="row">';
    
    assets.forEach(asset => {
        const deviceCount = asset.devices ? asset.devices.length : 0;
        const enabledDevices = asset.devices ? asset.devices.filter(d => d.isEnabled).length : 0;
        
        html += `
            <div class="col-md-6 col-lg-4 mb-3">
                <div class="asset-list-item h-100" onclick="openAssetModal('${asset.id}')">
                    <div class="d-flex flex-column h-100">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div class="flex-grow-1">
                                <h6 class="mb-1 fw-bold">${asset.name}</h6>
                                <small class="text-muted">ID: ${asset.id}</small>
                            </div>
                            <div class="asset-status">
                                <span class="status-dot ${asset.isEnabled ? '' : 'disabled'}"></span>
                                <span class="badge ${asset.isEnabled ? 'bg-success' : 'bg-secondary'} ms-1">
                                    ${asset.isEnabled ? 'Active' : 'Inactive'}
                                </span>
                            </div>
                        </div>
                        
                        <div class="flex-grow-1">
                            <div class="d-flex align-items-center text-muted small mb-1">
                                <i class="bi bi-cpu me-1"></i>
                                <span>${deviceCount} device${deviceCount !== 1 ? 's' : ''}</span>
                                ${deviceCount > 0 ? `<span class="ms-2 text-success">(${enabledDevices} enabled)</span>` : ''}
                            </div>
                            <div class="d-flex align-items-center text-muted small">
                                <i class="bi bi-clock me-1"></i>
                                <span>${asset.interval}ms interval</span>
                            </div>
                        </div>
                        
                        <div class="mt-2 pt-2 border-top">
                            <small class="text-primary">
                                <i class="bi bi-cursor-pointer me-1"></i>Click to configure
                            </small>
                        </div>
                    </div>
                </div>
            </div>`;
    });
    
    html += '</div>';
    container.innerHTML = html;
}

let currentAsset = null;

function openAssetModal(assetId) {
    fetch('Configuration/GetAssets')
        .then(response => response.json())
        .then(data => {
            if (data.success && data.assets) {
                const asset = data.assets.find(a => a.id === assetId);
                if (asset) {
                    currentAsset = asset;
                    populateAssetModal(asset);
                    const modal = new bootstrap.Modal(document.getElementById('assetModal'));
                    modal.show();
                }
            }
        })
        .catch(error => {
            console.error('Error loading asset details:', error);
            showToast('Error loading asset details', 'danger');
        });
}

function populateAssetModal(asset) {
    document.getElementById('modalAssetName').textContent = asset.name;
    document.getElementById('modalAssetId').textContent = asset.id;
    document.getElementById('modalAssetInterval').textContent = asset.interval + 'ms';
    document.getElementById('modalAssetEnabled').checked = asset.isEnabled;
    
    const deviceCount = asset.devices ? asset.devices.length : 0;
    document.getElementById('deviceCount').textContent = deviceCount;
    
    const devicesContainer = document.getElementById('modalDevicesContainer');
    
    if (!asset.devices || asset.devices.length === 0) {
        devicesContainer.innerHTML = `
            <div class="text-center text-muted py-3">
                <i class="bi bi-cpu-fill display-4 mb-2"></i>
                <p class="mb-0">No devices found for this asset</p>
            </div>`;
        return;
    }
    
    let devicesHtml = '';
    asset.devices.forEach(device => {
        devicesHtml += `
            <div class="card mb-2">
                <div class="card-body p-3">
                    <div class="d-flex justify-content-between align-items-center">
                        <div class="flex-grow-1">
                            <div class="d-flex align-items-center mb-1">
                                <i class="bi bi-cpu text-muted me-2"></i>
                                <span class="fw-semibold">${device.name}</span>
                            </div>
                            <small class="text-muted">ID: ${device.id} • Interval: ${device.interval}ms</small>
                        </div>
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" 
                                   id="modalDevice_${device.id}" 
                                   ${device.isEnabled ? 'checked' : ''}
                                   data-device-id="${device.id}">
                            <label class="form-check-label" for="modalDevice_${device.id}">
                                <span class="badge ${device.isEnabled ? 'bg-success' : 'bg-secondary'}">
                                    ${device.isEnabled ? 'Enabled' : 'Disabled'}
                                </span>
                            </label>
                        </div>
                    </div>
                </div>
            </div>`;
    });
    
    devicesContainer.innerHTML = devicesHtml;
    
    // Add event listeners for device toggles
    asset.devices.forEach(device => {
        const checkbox = document.getElementById(`modalDevice_${device.id}`);
        checkbox.addEventListener('change', function() {
            const badge = this.parentElement.querySelector('.badge');
            if (this.checked) {
                badge.className = 'badge bg-success';
                badge.textContent = 'Enabled';
            } else {
                badge.className = 'badge bg-secondary';
                badge.textContent = 'Disabled';
            }
        });
    });
}

function saveAssetChanges() {
    if (!currentAsset) return;
    
    const assetEnabled = document.getElementById('modalAssetEnabled').checked;
    const saveBtn = document.querySelector('#assetModal .btn-primary');
    
    // Show loading state
    const originalText = saveBtn.innerHTML;
    saveBtn.innerHTML = '<i class="bi bi-arrow-clockwise spin me-1"></i>Saving...';
    saveBtn.disabled = true;
    
    // Collect all changes
    const promises = [];
    
    // Update asset enabled state
    if (assetEnabled !== currentAsset.isEnabled) {
        promises.push(
            fetch('Configuration/UpdateAssetEnabled', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `assetId=${encodeURIComponent(currentAsset.id)}&isEnabled=${assetEnabled}`
            }).then(response => response.json())
        );
    }
    
    // Update device enabled states
    if (currentAsset.devices) {
        currentAsset.devices.forEach(device => {
            const checkbox = document.getElementById(`modalDevice_${device.id}`);
            if (checkbox && checkbox.checked !== device.isEnabled) {
                promises.push(
                    fetch('Configuration/UpdateDeviceEnabled', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: `assetId=${encodeURIComponent(currentAsset.id)}&deviceId=${encodeURIComponent(device.id)}&isEnabled=${checkbox.checked}`
                    }).then(response => response.json())
                );
            }
        });
    }
    
    // Execute all updates
    Promise.all(promises)
        .then(results => {
            const hasErrors = results.some(result => !result.success);
            if (hasErrors) {
                showToast('Some changes could not be saved', 'warning');
            } else {
                showToast('Changes saved successfully', 'success');
                // Close modal and refresh assets
                bootstrap.Modal.getInstance(document.getElementById('assetModal')).hide();
                loadAssets();
            }
        })
        .catch(error => {
            console.error('Error saving changes:', error);
            showToast('Error saving changes', 'danger');
        })
        .finally(() => {
            // Restore button state
            saveBtn.innerHTML = originalText;
            saveBtn.disabled = false;
        });
}

function showAlert(message, type) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    
    // Insert at the top of the container
    const container = document.querySelector('.container');
    container.insertAdjacentHTML('afterbegin', alertHtml);
}

function showToast(message, type) {
    // Simple toast notification
    const toast = document.createElement('div');
    toast.className = `alert alert-${type} position-fixed top-0 end-0 m-3`;
    toast.style.zIndex = '9999';
    toast.innerHTML = `
        ${message}
        <button type="button" class="btn-close ms-2" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(toast);
    
    // Auto remove after 3 seconds
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, 3000);
}

// Load assets when page loads
document.addEventListener('DOMContentLoaded', function() {
    loadAssets();
});

// Add spin animation for loading indicators
const style = document.createElement('style');
style.textContent = `
    .spin {
        animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
`;
document.head.appendChild(style);