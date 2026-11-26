import { apiKey, requireLogin } from './account.js';

let currentRequestId = 0;
let activeWorkshop = null;

function getPanel() {
  return (
    document.getElementById('workshop-details') ||
    document.getElementById('detail-panel') ||
    document.querySelector('.detail-panel')
  );
}

function getFieldsContainer(panel) {
  return (
    panel.querySelector('#detail-facts') ||
    panel.querySelector('[data-detail-fields]') ||
    panel.querySelector('dl') ||
    panel.querySelector('.detail-fields') ||
    panel
  );
}

function getEditForm() {
  return document.getElementById('edit-form') || document.getElementById('edit-workshop-form');
}

function getFieldControl(form, name) {
  if (!form) return null;
  return (
    form.elements.namedItem(name) ||
    form.querySelector(`[name="${name}"], [id="edit-${name}"], [id="${name}"]`)
  );
}

function clearDetail(panel, message = '') {
  const heading = panel.querySelector('#detail-title, h2, h3, [data-detail-title]');
  const desc = panel.querySelector('#detail-description, [data-detail-description], .detail-description, p');
  const status = panel.querySelector('#detail-status, [data-detail-status], .detail-status');
  const editBtn = panel.querySelector('#start-edit');
  const editForm = getEditForm();

  if (heading) heading.textContent = '';
  if (desc) desc.textContent = '';
  if (status) status.textContent = message;
  if (editBtn) editBtn.hidden = true;
  if (editForm) editForm.hidden = true;

  const container = getFieldsContainer(panel);
  container.querySelectorAll('dt, dd').forEach((el) => el.remove());
  return { heading, desc, status, container, editBtn, editForm };
}

function appendField(container, label, value) {
  const dt = document.createElement('dt');
  dt.textContent = label;
  const dd = document.createElement('dd');
  dd.textContent = value ?? '';
  container.appendChild(dt);
  container.appendChild(dd);
}

function populateEditForm(form, workshop) {
  if (!form || !workshop) return;
  const map = {
    title: workshop.title ?? '',
    category: workshop.category ?? '',
    date: workshop.date ?? '',
    room: workshop.room ?? '',
    description: workshop.description ?? '',
    level: workshop.level ?? '',
    duration: workshop.duration ?? 0,
    seatsLeft: workshop.seatsLeft ?? 0,
  };
  for (const [key, val] of Object.entries(map)) {
    const ctrl = getFieldControl(form, key);
    if (ctrl) ctrl.value = val;
  }
}

function wireEditControls(panel) {
  const editBtn = panel.querySelector('#start-edit');
  const editForm = getEditForm();
  const statusEl = panel.querySelector('#detail-status, [data-detail-status], .detail-status');

  if (editBtn && !editBtn.dataset.wired) {
    editBtn.dataset.wired = 'true';
    editBtn.addEventListener('click', () => {
      if (!activeWorkshop || !editForm) return;
      if (typeof requireLogin === 'function' && !apiKey) {
        requireLogin();
      }
      populateEditForm(editForm, activeWorkshop);
      editForm.hidden = false;
      if (statusEl) statusEl.textContent = '';
    });
  }

  if (editForm && !editForm.dataset.wired) {
    editForm.dataset.wired = 'true';
    editForm.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!activeWorkshop) return;

      if (typeof requireLogin === 'function' && !apiKey) {
        requireLogin();
      }
      if (!apiKey) {
        if (statusEl) statusEl.textContent = 'Sign in first to obtain an API key.';
        return;
      }

      const payload = {
        title: String(getFieldControl(editForm, 'title')?.value ?? '').trim(),
        category: String(getFieldControl(editForm, 'category')?.value ?? '').trim(),
        date: String(getFieldControl(editForm, 'date')?.value ?? '').trim(),
        room: String(getFieldControl(editForm, 'room')?.value ?? '').trim(),
        description: String(getFieldControl(editForm, 'description')?.value ?? '').trim(),
        level: String(getFieldControl(editForm, 'level')?.value ?? '').trim(),
        duration: Number(getFieldControl(editForm, 'duration')?.value ?? 0),
        seatsLeft: Number(getFieldControl(editForm, 'seatsLeft')?.value ?? 0),
      };

      const res = await fetch('/api/workshops/' + encodeURIComponent(activeWorkshop.id), {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'X-API-Key': apiKey,
        },
        body: JSON.stringify(payload),
      });

      if (res.status === 204) {
        editForm.hidden = true;
        if (statusEl) statusEl.textContent = 'Saved.';
        await loadWorkshopDetail(activeWorkshop.id, ++currentRequestId);
        window.dispatchEvent(new CustomEvent('workshops:updated', { detail: { id: activeWorkshop.id } }));
        return;
      }

      if (res.status === 401) {
        if (statusEl) statusEl.textContent = 'Unauthorized (401). Sign in again.';
        return;
      }

      if (res.status === 404) {
        if (statusEl) statusEl.textContent = 'Workshop not found (404).';
        return;
      }

      const err = await res.json().catch(() => null);
      if (statusEl) statusEl.textContent = err?.title || `Update failed (${res.status}).`;
    });
  }
}

async function loadWorkshopDetail(id, requestId) {
  const panel = getPanel();
  if (!panel) return;
  wireEditControls(panel);

  panel.hidden = false;
  panel.style.display = '';
  const { heading, desc, status, container, editBtn } = clearDetail(panel, 'Loading...');

  try {
    const response = await fetch('/api/workshops/' + encodeURIComponent(id));
    if (requestId !== currentRequestId) return;

    if (response.status === 404) {
      activeWorkshop = null;
      clearDetail(panel, 'Workshop not found');
      return;
    }

    if (!response.ok) {
      activeWorkshop = null;
      clearDetail(panel, 'Unable to load workshop details.');
      return;
    }

    const workshop = await response.json();
    if (requestId !== currentRequestId) return;
    activeWorkshop = workshop;

    if (status) status.textContent = '';
    if (heading) heading.textContent = workshop.title ?? '';
    if (desc) desc.textContent = workshop.description ?? '';
    if (editBtn) editBtn.hidden = false;

    const dl = container.tagName === 'DL' ? container : document.createElement('dl');
    dl.querySelectorAll('dt, dd').forEach((el) => el.remove());

    const fields = [
      ['category', workshop.category],
      ['date', workshop.date],
      ['room', workshop.room],
      ['level', workshop.level],
      ['duration', workshop.duration],
      ['seatsLeft', workshop.seatsLeft],
    ];

    for (const [label, val] of fields) {
      appendField(dl, label, val);
    }

    if (dl !== container) {
      container.appendChild(dl);
    }
  } catch {
    if (requestId !== currentRequestId) return;
    activeWorkshop = null;
    clearDetail(panel, 'Unable to load workshop details.');
  }
}

function handleHashChange() {
  const panel = getPanel();
  if (!panel) return;

  const hash = location.hash || '';
  if (hash === '#workshops' || hash === '') {
    currentRequestId += 1;
    activeWorkshop = null;
    panel.hidden = true;
    panel.style.display = 'none';
    return;
  }

  const match = /^#workshop-(\d+)$/.exec(hash);
  if (match) {
    currentRequestId += 1;
    loadWorkshopDetail(match[1], currentRequestId);
  }
}

window.addEventListener('hashchange', handleHashChange);
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', handleHashChange, { once: true });
} else {
  handleHashChange();
}
