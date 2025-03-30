// site.js
let currentBooks = [];
let currentPage = 1;
let isLoading = false;

document.addEventListener("DOMContentLoaded", function () {
    // Set table view as default
    localStorage.removeItem("preferredView");
    
    // Explicitly show table and hide gallery
    document.getElementById("bookTable").classList.remove("d-none");
    document.getElementById("bookGallery").classList.add("d-none");
    document.getElementById("toggleView").textContent = "Gallery View";
    

    // DOM elements
    const bookTableBody = document.getElementById("bookTableBody");
    const bookGallery = document.getElementById("bookGallery");
    const toggleViewBtn = document.getElementById("toggleView");
    const loadingIndicator = createLoadingIndicator();
    
    // Initial load
    loadBooks(true);

    // Event Listeners
    document.getElementById("avgLikes").addEventListener("input", function () {
        document.getElementById("likesValue").textContent = this.value;
        loadBooks(true);
    });

    setupInfiniteScroll();
    setupExportButton();
    setupToggleView();
    setupRandomSeed();
    setupParameterChangeListeners();
        
    async function fetchRandomBookImage() {
        try {
            const response = await fetch("https://picsum.photos/400/300");
            if (!response.ok) throw new Error("Image fetch failed");
            return response.url;
        } catch (error) {
            console.error("Failed to fetch image:", error);
            return "https://via.placeholder.com/400x300?text=Book+Cover";
        }
    }    
    
    function createBookRow(book) {
        const row = document.createElement("tr");
        row.className = "book-row";
        row.innerHTML = `
            <td>${book.index}</td>
            <td>${book.isbn}</td>
            <td>${book.title}</td>
            <td>${book.author}</td>
            <td>${book.publisher || 'Unknown'}</td>
            <td>${book.actualLikes || 0}</td>
            <td>${book.reviews?.length || 0}</td>
        `;
    
        row.addEventListener("click", function() {
            // Toggle existing review
            const existingReview = row.nextElementSibling;
            if (existingReview?.classList.contains("review-row")) {
                existingReview.remove();
                return;
            }
    
            // Show loading state
            const loadingRow = document.createElement("tr");
            loadingRow.className = "review-row loading";
            loadingRow.innerHTML = `
                <td colspan="7" class="text-center">
                    <div class="spinner-border spinner-border-sm" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    Loading review...
                </td>
            `;
            row.insertAdjacentElement("afterend", loadingRow);
    
            // Build reviews HTML
            let reviewsHtml = '';
            if (book.reviews?.length > 0 && book.reviewers?.length > 0) {
                const reviewCount = Math.min(book.reviews.length, book.reviewers.length);
                for (let i = 0; i < reviewCount; i++) {
                    reviewsHtml += `
                        <div class="review-item mb-3 p-3 bg-light rounded">
                            <div class="review-text">${book.reviews[i]}</div>
                            <div class="reviewer-name text-muted mt-2">
                                <span class="fw-bold">${book.reviewers[i]}</span>
                                <span class="ms-2 badge bg-secondary">${getRandomReviewerTitle()}</span>
                            </div>
                        </div>
                    `;
                }
            } else {
                reviewsHtml = '<div class="text-muted p-3">No reviews available</div>';
            }
    
            // Create the actual review row
            setTimeout(() => {
                loadingRow.innerHTML = `
                    <td colspan="7">
                        <div class="review-content d-flex align-items-start gap-4">
                            <div class="book-cover-container position-relative">
                                <img src="${book.coverImageUrl}" alt="${book.title}" 
                                     class="book-review-image rounded" 
                                     onerror="this.src='https://via.placeholder.com/300x450?text=Cover+Not+Available'">
                                <div class="book-cover-overlay p-3">
                                    <h5 class="book-title">${book.title}</h5>
                                    <p class="book-author">by ${book.author}</p>
                                </div>
                            </div>
                            <div class="reviews-container flex-grow-1">
                                <h5 class="mb-3">Customer Reviews</h5>
                                ${reviewsHtml}
                            </div>
                        </div>
                    </td>
                `;
                
                // Add fade-in effect
                setTimeout(() => {
                    loadingRow.querySelector('.review-content').style.opacity = 1;
                }, 10);
            }, 500);
        });
    
        return row;
    }
    
    // Helper function to generate random reviewer titles
    function getRandomReviewerTitle() {
        const titles = [
            "Verified Buyer", "Top Contributor", "Book Enthusiast", 
            "Literary Critic", "Avid Reader", "Editor's Choice",
            "Regular Customer", "Book Club Member", "Library Staff"
        ];
        return titles[Math.floor(Math.random() * titles.length)];
    }
    
    // Helper function to enhance review text
    function enhanceReviewText(review, locale) {
        // Add more sophisticated formatting based on locale
        if (locale === "de") {
            return review.replace(/\.$/, '!').replace(/^(\w)/, match => match.toUpperCase());
        } else if (locale === "fr") {
            return review.replace(/\.$/, '!').replace(/^(\w)/, match => match.toUpperCase());
        }
        return review.charAt(0).toUpperCase() + review.slice(1);
    }

    function createLoadingIndicator() {
        const indicator = document.createElement("div");
        indicator.id = "loadingIndicator";
        indicator.className = "loading-overlay d-none";
        indicator.innerHTML = `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p>Loading books...</p>
        `;
        document.body.appendChild(indicator);
        return indicator;
    }

    function setupInfiniteScroll() {
        let scrollTimeout;
        window.addEventListener("scroll", function () {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 500 && !isLoading) {
                    loadBooks(false);
                }
            }, 250);
        });
    }

    function setupExportButton() {
        document.getElementById("exportCsv").addEventListener("click", async function() {
            const btn = this;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Exporting...';
            
            const request = {
                locale: document.getElementById("locale").value,
                seed: parseInt(document.getElementById("seed").value),
                avgLikes: parseFloat(document.getElementById("avgLikes").value),
                avgReviews: parseFloat(document.getElementById("avgReviews").value),
                page: 1,
                pageSize: 1000
            };
            
            window.location.href = `/api/books/export?${new URLSearchParams(request).toString()}`;
            
            setTimeout(() => {
                btn.disabled = false;
                btn.textContent = "Export to CSV";
            }, 3000);
        });
    }

    function setupToggleView() {
        document.getElementById("toggleView").addEventListener("click", function () {
            const table = document.getElementById("bookTable");
            const gallery = document.getElementById("bookGallery");
            const toggleBtn = this;
            
            // Disable during transition
            toggleBtn.disabled = true;
            
            // Toggle with animation
            table.style.opacity = table.classList.contains("d-none") ? '1' : '0';
            gallery.style.opacity = gallery.classList.contains("d-none") ? '1' : '0';
            
            setTimeout(() => {
                table.classList.toggle("d-none");
                gallery.classList.toggle("d-none");
                
                // Update UI
                const isGalleryView = !table.classList.contains("d-none");
                toggleBtn.textContent = isGalleryView ? "Table View" : "Gallery View";
                localStorage.setItem("preferredView", isGalleryView ? "gallery" : "table");
                
                // Render if empty
                if (!gallery.classList.contains("d-none") && gallery.children.length === 0 && currentBooks.length > 0) {
                    renderBooks(currentBooks, true);
                }
                
                toggleBtn.disabled = false;
            }, 50);
        });
    }

    function setupRandomSeed() {
        document.getElementById("randomSeed").addEventListener("click", function () {
            document.getElementById("seed").value = Math.floor(Math.random() * 10000);
            loadBooks(true);
        });
    }

    function setupParameterChangeListeners() {
        ["locale", "avgReviews", "seed"].forEach(id => {
            document.getElementById(id).addEventListener("change", () => loadBooks(true));
        });
    }

    async function loadBooks(reset = false) {
        if (isLoading && !reset) return;
        isLoading = true;
        loadingIndicator.classList.remove("d-none");
        
        try {
            if (reset) {
                currentPage = 1;
                bookTableBody.innerHTML = "";
                bookGallery.innerHTML = "";
            }
            
            const request = {
                locale: document.getElementById("locale").value,
                seed: parseInt(document.getElementById("seed").value),
                avgLikes: parseFloat(document.getElementById("avgLikes").value),
                avgReviews: parseFloat(document.getElementById("avgReviews").value),
                page: currentPage,
                pageSize: 20,
                galleryView: !document.getElementById("bookGallery").classList.contains("d-none")
            };
    
            const queryString = new URLSearchParams(request).toString();
            const response = await fetch(`/api/books?${queryString}`);
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.errors?.join("\n") || error.error || "Failed to load books");
            }
    
            const data = await response.json();
            currentBooks = reset ? data : [...currentBooks, ...data];
            renderBooks(data, request.galleryView, reset);
            currentPage++;
        } catch (error) {
            console.error("Error loading books:", error);
            showToast(error.message, "danger");
        } finally {
            isLoading = false;
            loadingIndicator.classList.add("d-none");
        }
    }

    function renderBooks(books, forceGallery = false, reset = false) {
        const isGallery = forceGallery || document.getElementById("bookTable").classList.contains("d-none");
        
        if (books.length === 0 && reset) {
            const noResults = document.createElement("div");
            noResults.className = "alert alert-info";
            noResults.textContent = "No books found with these parameters";
            document.getElementById("bookContainer").appendChild(noResults);
            return;
        }
        
        if (isGallery) {
            books.forEach(book => {
                bookGallery.appendChild(createBookCard(book));
            });
        } else {
            books.forEach(book => {
                bookTableBody.appendChild(createBookRow(book));
            });
        }
    }

    function createBookCard(book) {
        const col = document.createElement("div");
        col.className = "col-md-4 mb-4";
        col.innerHTML = `
            <div class="card h-100">
                <img src="${book.coverImageUrl}" 
                     class="card-img-top" 
                     alt="${book.title}"
                     loading="lazy"
                     style="height: 350px; object-fit: cover;">
                <div class="card-body">
                    <h5 class="card-title">${book.title}</h5>
                    <p class="card-text text-muted">by ${book.author}</p>
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="badge bg-primary">${book.actualLikes || 0} ♥</span>
                        <small class="text-muted">${book.reviews?.length || 0} reviews</small>
                    </div>
                </div>
            </div>
        `;
    
        // Add click handler to show reviews in a modal
        col.querySelector('.card').addEventListener('click', function() {
            const lang = book.locale === "de" ? "German" : 
                         book.locale === "fr" ? "French" : "English";
            
            let reviewsHtml = '';
            if (book.reviews?.length > 0 && book.reviewers?.length > 0) {
                const reviewCount = Math.min(book.reviews.length, book.reviewers.length);
                for (let i = 0; i < reviewCount; i++) {
                    reviewsHtml += `
                        <div class="review-item mb-3">
                            <div class="review-text">${book.reviews[i]}</div>
                            <div class="reviewer-name text-muted">— ${book.reviewers[i]}</div>
                        </div>
                    `;
                }
            } else {
                reviewsHtml = '<div class="text-muted">No reviews available</div>';
            }
    
            const modalHtml = `
                <div class="modal fade" id="bookReviewsModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title">${book.title} Reviews (${lang})</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <div class="text-center mb-3">
                                    <img src="${book.coverImageUrl || 'https://via.placeholder.com/300x450?text=No+Cover'}" 
                                         class="img-fluid rounded" 
                                         style="max-height: 200px; width: auto;">
                                </div>
                                <div class="reviews-container">
                                    ${reviewsHtml}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
    
            // Create and show modal
            const modalContainer = document.createElement('div');
            modalContainer.innerHTML = modalHtml;
            document.body.appendChild(modalContainer);
            
            const modal = new bootstrap.Modal(modalContainer.querySelector('.modal'));
            modal.show();
            
            // Remove modal after it's closed
            modalContainer.querySelector('.modal').addEventListener('hidden.bs.modal', function() {
                document.body.removeChild(modalContainer);
            });
        });
    
        return col;
    }
    
    function generateCSV(books) {
        const headers = "Index,ISBN,Title,Author,Publisher,Likes,Reviews\n";
        const rows = books.map(book => 
            `${book.index},"${book.isbn}","${book.title.replace(/"/g, '""')}","${book.author.replace(/"/g, '""')}",` +
            `"${(book.publisher || 'Unknown').replace(/"/g, '""')}",${book.actualLikes || 0},${book.reviews?.length || 0}`
        ).join("\n");
        return headers + rows;
    }

    function downloadCSV(content, filename) {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }

    function showToast(message, type = "info") {
        // Remove any existing toasts
        document.querySelectorAll('.toast').forEach(toast => toast.remove());
        
        const toast = document.createElement("div");
        toast.className = `toast show align-items-center text-bg-${type} border-0`;
        toast.style.maxWidth = "500px";
        toast.style.width = "90%";
        toast.style.margin = "10px auto";
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body" style="white-space: pre-wrap;">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 5000);
    }
});