import { apiKey, requireLogin } from './account.js';

const endpoint = '/api/chat';
const listEl = document.querySelector('#chat-messages');
const formEl = document.querySelector('#chat-form');
const inputEl = document.querySelector('#chat-input');
const statusEl = document.querySelector('#chat-status');

function getWorkshopIdFromHash() {
  const match = /^#workshop-(\d+)$/.exec(location.hash || '');
  return match ? Number(match[1]) : null;
}

function renderMessages(messages) {
  if (!listEl) return;
  const items = (messages ?? []).map((msg) => {
    const li = document.createElement('li');
    const sender = msg.sender ?? 'User';
    const text = msg.text ?? '';
    const scope = msg.workshopId ? ` [Workshop #${msg.workshopId}]` : '';
    li.textContent = `${sender}${scope}: ${text}`;
    return li;
  });
  listEl.replaceChildren(...items);
}

async function loadMessages() {
  if (!listEl) return;
  const workshopId = getWorkshopIdFromHash();
  const url = new URL(endpoint, location.origin);
  if (workshopId !== null) {
    url.searchParams.set('workshopId', String(workshopId));
  }

  try {
    const res = await fetch(url, { cache: 'no-store' });
    if (!res.ok) throw new Error('HTTP ' + res.status);
    const data = await res.json();
    renderMessages(data);
    if (statusEl && data.length === 0) {
      statusEl.textContent = 'No messages yet.';
    } else if (statusEl) {
      statusEl.textContent = '';
    }
  } catch {
    if (statusEl) statusEl.textContent = 'Unable to load chat messages.';
  }
}

formEl?.addEventListener('submit', async (event) => {
  event.preventDefault();
  const text = (inputEl?.value ?? '').trim();
  if (!text) return;

  if (!apiKey) {
    if (typeof requireLogin === 'function') requireLogin();
    if (statusEl) statusEl.textContent = 'Sign in first to send a message.';
    return;
  }

  const payload = {
    text,
    workshopId: getWorkshopIdFromHash(),
  };

  try {
    const res = await fetch(endpoint, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-API-Key': apiKey,
      },
      body: JSON.stringify(payload),
    });

    if (res.status === 401) {
      if (typeof requireLogin === 'function') requireLogin();
      if (statusEl) statusEl.textContent = 'Unauthorized (401). Sign in again.';
      return;
    }

    if (!res.ok) {
      const err = await res.json().catch(() => null);
      if (statusEl) statusEl.textContent = err?.title || `Send failed (${res.status}).`;
      return;
    }

    if (inputEl) inputEl.value = '';
    await loadMessages();
  } catch {
    if (statusEl) statusEl.textContent = 'Could not send message.';
  }
});

window.addEventListener('hashchange', loadMessages);
loadMessages();
