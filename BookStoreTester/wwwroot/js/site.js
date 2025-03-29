let currentBooks = [];
let currentPage = 1;
let isLoading = false;

document.addEventListener("DOMContentLoaded", function () {
    // Initialize view from localStorage
    const preferredView = localStorage.getItem("preferredView") || "table";
    if (preferredView === "gallery") {
        document.getElementById("bookTable").classList.add("d-none");
        document.getElementById("bookGallery").classList.remove("d-none");
        document.getElementById("toggleView").textContent = "Table View";
    }

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

    async function generateReview(language) {
        try {
        const response = await fetch('/api/generate-review', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ language })
        });
        const data = await response.json();
        return data.review || "Could not generate review.";
        } catch (error) {
        console.error("Review generation failed:", error);
        return "Review service unavailable.";
        }
    }
        
        async function fetchRandomBookImage() {
            try {
                const response = await fetch("https://source.unsplash.com/400x300/?book,reading,library");
                if (!response.ok) throw new Error("Image fetch failed");
                return response.url;
            } catch (error) {
                console.error("Failed to fetch image:", error);
                return "https://via.placeholder.com/400x300?text=Book+Cover";
            }
        }
    
    function createBookRow(book) {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${book.index}</td>
            <td>${book.isbn}</td>
            <td>${book.title}</td>
            <td>${book.author}</td>
            <td>${book.publisher || 'Unknown'}</td>
            <td>${book.actualLikes || 0}</td>
            <td>${book.reviews?.length || 0}</td>
        `;
    
        row.addEventListener("click", async function () {
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
    
            try {
                const lang = ["English", "German", "French"][Math.floor(Math.random() * 3)];
                const [reviewText, imageUrl] = await Promise.all([
                    generateReview(lang), 
                    fetchRandomBookImage()
                ]);
    
                loadingRow.innerHTML = `
                    <td colspan="7">
                        <div class="review-content">
                            <strong>Review (${lang}):</strong> ${reviewText} <br>
                            <img src="${imageUrl}" alt="Book Image" class="book-review-image">
                        </div>
                    </td>
                `;
            } catch (error) {
                loadingRow.innerHTML = `
                    <td colspan="7" class="text-danger">
                        Failed to load review. Please try again.
                    </td>
                `;
            }
        });
    
        return row;
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
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Exporting...';
            
            try {
                const request = new URLSearchParams({
                    locale: document.getElementById("locale").value,
                    seed: document.getElementById("seed").value,
                    avgLikes: document.getElementById("avgLikes").value,
                    avgReviews: document.getElementById("avgReviews").value,
                    page: 1,
                    pageSize: 1000
                }).toString();
                
                const response = await fetch(`/api/books?${request}`);
                if (!response.ok) throw new Error("Export failed");
                const books = await response.json();
                
                // Generate CSV
                const csvContent = generateCSV(books);
                downloadCSV(csvContent, "books.csv");
            } catch (error) {
                console.error("Export error:", error);
                showToast("Export failed. Please try again.", "danger");
            } finally {
                btn.disabled = false;
                btn.textContent = "Export to CSV";
            }
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
            
            const request = new URLSearchParams({
                locale: document.getElementById("locale").value,
                seed: document.getElementById("seed").value,
                avgLikes: document.getElementById("avgLikes").value,
                avgReviews: document.getElementById("avgReviews").value,
                page: currentPage,
                pageSize: 20
            }).toString();
            
            const response = await fetch(`/api/books?${request}`);
            if (!response.ok) throw new Error("Failed to load books");
            const data = await response.json();
            
            currentBooks = reset ? data : [...currentBooks, ...data];
            renderBooks(data, false, reset);
            currentPage++;
        } catch (error) {
            console.error("Error loading books:", error);
            showToast("Failed to load books", "danger");
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
                <img src="${book.coverImageUrl || 'https://via.placeholder.com/300x450?text=No+Cover'}" 
                     class="card-img-top" 
                     alt="${book.title}"
                     loading="lazy">
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
        const toast = document.createElement("div");
        toast.className = `toast show align-items-center text-bg-${type} border-0`;
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 3000);
    }
});