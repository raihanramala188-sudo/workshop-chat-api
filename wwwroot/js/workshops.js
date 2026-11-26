import { apiKey, requireLogin } from './account.js';

const endpoint = '/api/workshops';
const catalog = document.querySelector('#catalog');
const catalogStatus = document.querySelector('#status');
const filter = document.querySelector('#category-filter');
const filters = document.querySelector('#filters');
const cardTemplate = document.querySelector('#card-template');
const form = document.querySelector('#workshop-form');
const createStatus = document.querySelector('#create-status');
let pendingRead;

function renderCard(workshop) {
    const card = cardTemplate.content.cloneNode(true);
    // textContent keeps titles and descriptions as plain text, including any angle brackets.
    card.querySelector('.topic').textContent = workshop.category;
const link = card.querySelector('.detail-link');
if (link) {
  link.textContent = workshop.title;
  link.href = '#workshop-' + workshop.id;
}
    const date = card.querySelector('time');
    date.dateTime = workshop.date;
    date.textContent = workshop.date;
    card.querySelector('.room').textContent = workshop.room;
    card.querySelector('.description').textContent = workshop.description;
    card.querySelector('.details').textContent =
        workshop.level + ' · ' + workshop.duration + ' min · ' + workshop.seatsLeft + ' seats left';
    return card;
}

function updateCategories(workshops) {
    const names = [...new Set(workshops.map(workshop => workshop.category))].sort();
    filter.replaceChildren(new Option('All categories', ''), ...names.map(name => new Option(name, name)));
    document.querySelector('#categories').replaceChildren(...names.map(name => new Option(name, name)));
}

async function loadCatalog() {
    // A slower response for a previous filter must not replace the latest selection.
    pendingRead?.abort();
    pendingRead = new AbortController();
    const signal = pendingRead.signal;
    const category = filter.value;
    const url = new URL(endpoint, location.origin);
    if (category) url.searchParams.set('category', category);

    catalogStatus.textContent = 'Loading workshops…';
    catalog.setAttribute('aria-busy', 'true');
    try {
        const response = await fetch(url, { signal, cache: 'no-store' });
        if (!response.ok) throw new Error('HTTP ' + response.status);
        const workshops = await response.json();
        if (!category) updateCategories(workshops);
        catalog.replaceChildren(...workshops.map(renderCard));
        catalogStatus.textContent = workshops.length === 0 ? 'No workshops match your filter.'
            : workshops.length + (workshops.length === 1 ? ' workshop shown.' : ' workshops shown.');
    } catch (error) {
        if (signal.aborted) return;
        catalog.replaceChildren();
        catalogStatus.textContent = 'Could not load the catalogue. Select Apply filter to retry.';
    } finally {
        if (!signal.aborted) catalog.setAttribute('aria-busy', 'false');
    }
}

filters.addEventListener('submit', event => {
    event.preventDefault();
    loadCatalog();
});
filter.addEventListener('change', () => filters.requestSubmit());

form.addEventListener('submit', async event => {
    event.preventDefault();
    const fields = form.querySelector('fieldset');
    if (fields.disabled) return;
    // FormData contains strings. Send JSON numbers for the two integer properties.
    const input = Object.fromEntries(new FormData(form));
    input.duration = form.elements.duration.valueAsNumber;
    input.seatsLeft = form.elements.seatsLeft.valueAsNumber;
    fields.disabled = true;
    createStatus.textContent = 'Saving workshop…';
    try {
        const response = await fetch(endpoint, {
            method: 'POST',
            // Browsing is public. Only the write request carries the key from login.
            headers: { 'Content-Type': 'application/json', 'X-API-Key': apiKey },
            body: JSON.stringify(input)
        });
        if (response.status === 401) {
            requireLogin();
            createStatus.textContent = '';
            return;
        }
        if (response.status === 400) {
            const problem = await response.json();
            const messages = Object.values(problem.errors ?? {}).flat();
            createStatus.textContent = messages.join(' ') || 'Check the workshop fields.';
            return;
        }
        if (response.status !== 201) throw new Error('Unexpected response');
        const saved = await response.json();
        createStatus.textContent = 'Created “' + saved.title + '” (workshop ' + saved.id + ').';
        form.reset();
        // Read from SQLite again, including the new category even if another filter was active.
        filter.value = '';
        await loadCatalog();
    } catch {
        createStatus.textContent = 'Could not confirm creation. Refresh the catalogue before trying again.';
    } finally {
        fields.disabled = false;
    }
});

loadCatalog();


window.addEventListener('workshops:updated', () => {
loadCatalog();
});
