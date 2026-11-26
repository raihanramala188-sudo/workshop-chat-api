// Keep the API key in this page's memory. Reloading the page clears the browser's copy.
export let apiKey = '';
const login = document.querySelector('#login');
const signup = document.querySelector('#signup');
const status = document.querySelector('#account-status');

export function requireLogin() {
    apiKey = '';
    document.querySelector('#api-key').textContent = '';
    document.querySelector('#key-details').hidden = true;
    document.querySelector('#workshop-form').hidden = true;
    login.hidden = false;
    status.textContent = 'Your API key is no longer valid. Sign in again.';
    login.elements.userName.focus();
}

async function submitAccount(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const fields = form.querySelector('fieldset');
    if (fields.disabled) return;
    const credentials = Object.fromEntries(new FormData(form));
    fields.disabled = true;
    status.textContent = '';
    try {
        const response = await fetch(form.action, {
            method: 'POST', headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(credentials)
        });
        if (!response.ok) {
            const problem = await response.json();
            const errors = Object.values(problem.errors ?? {}).flat();
            status.textContent = errors.join(' ') || problem.title || 'Please try again.';
            return;
        }
        form.elements.password.value = '';
        if (form === signup) {
            signup.hidden = true;
            status.textContent = 'Account created.';
            document.querySelector('#continue-login').hidden = false;
            return;
        }
        const account = await response.json();
        apiKey = account.apiKey;
        login.hidden = true;
        document.querySelector('#key-details').open = false;
        document.querySelector('#key-details').hidden = false;
        document.querySelector('#api-key').textContent = apiKey;
        document.querySelector('#workshop-form').hidden = false;
        status.textContent = 'Signed in as ' + account.userName + '.';
    } catch {
        status.textContent = 'Could not reach the account service. Please try again.';
    } finally {
        fields.disabled = false;
    }
}

signup?.addEventListener('submit', submitAccount);
login?.addEventListener('submit', submitAccount);
