(() => {
    if (document.getElementById('travelChatbot')) return;

    const root = document.createElement('aside');
    root.id = 'travelChatbot';
    root.className = 'travel-chatbot';
    root.innerHTML = `
        <button class="travel-chatbot-toggle" type="button" aria-label="Open travel assistant" aria-expanded="false">
            <i class="fas fa-sparkles"></i><span>Travel AI</span>
        </button>
        <section class="travel-chatbot-panel" aria-label="Travel AI assistant" hidden>
            <header class="travel-chatbot-header">
                <div><span class="travel-chatbot-kicker">Your trip co-pilot</span><strong>Travel Assistant</strong></div>
                <button class="travel-chatbot-close" type="button" aria-label="Close travel assistant"><i class="fas fa-xmark"></i></button>
            </header>
            <div class="travel-chatbot-messages" aria-live="polite">
                <div class="travel-chatbot-message assistant">Hi! I can help with destinations, itineraries, budgets, and packing plans.</div>
            </div>
            <div class="travel-chatbot-suggestions">
                <button type="button" data-prompt="Plan a 3-day trip for me">Plan a 3-day trip</button>
                <button type="button" data-prompt="Suggest a destination within my budget">Suggest a destination</button>
                <button type="button" data-prompt="What should I pack for my next trip?">Packing checklist</button>
            </div>
            <form class="travel-chatbot-form">
                <textarea rows="1" maxlength="2000" placeholder="Ask about your next adventure..." aria-label="Message"></textarea>
                <button type="submit" aria-label="Send message"><i class="fas fa-arrow-up"></i></button>
            </form>
        </section>`;
    document.body.appendChild(root);

    const toggle = root.querySelector('.travel-chatbot-toggle');
    const panel = root.querySelector('.travel-chatbot-panel');
    const close = root.querySelector('.travel-chatbot-close');
    const form = root.querySelector('.travel-chatbot-form');
    const input = form.querySelector('textarea');
    const messages = root.querySelector('.travel-chatbot-messages');

    const setOpen = open => {
        panel.hidden = !open;
        toggle.setAttribute('aria-expanded', String(open));
        root.classList.toggle('is-open', open);
        if (open) input.focus();
    };

    const addMessage = (text, role) => {
        const message = document.createElement('div');
        message.className = `travel-chatbot-message ${role}`;
        message.textContent = text;
        messages.appendChild(message);
        messages.scrollTop = messages.scrollHeight;
        return message;
    };

    const sendMessage = async message => {
        addMessage(message, 'user');
        const pending = addMessage('Thinking...', 'pending');
        input.disabled = true;
        try {
            const response = await fetch('/api/chat', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message })
            });
            const data = await response.json().catch(() => ({}));
            if (!response.ok) throw new Error(data.message || 'The assistant is unavailable right now.');
            pending.remove();
            addMessage(data.message, 'assistant');
        } catch (error) {
            pending.className = 'travel-chatbot-message error';
            pending.textContent = error.message || 'The assistant is unavailable right now.';
        } finally {
            input.disabled = false;
            input.focus();
        }
    };

    toggle.addEventListener('click', () => setOpen(panel.hidden));
    close.addEventListener('click', () => setOpen(false));
    root.querySelectorAll('[data-prompt]').forEach(button => {
        button.addEventListener('click', () => sendMessage(button.dataset.prompt));
    });
    form.addEventListener('submit', event => {
        event.preventDefault();
        const message = input.value.trim();
        if (!message || input.disabled) return;
        input.value = '';
        sendMessage(message);
    });
    input.addEventListener('keydown', event => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            form.requestSubmit();
        }
    });
})();
