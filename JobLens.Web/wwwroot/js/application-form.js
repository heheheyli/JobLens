(() => {
    const form = document.querySelector(".app-form");
    if (!form) return;

    const field = (name) => form.querySelector(`[name="${name}"]`);
    const fire = (el) => el.dispatchEvent(new Event("input", { bubbles: true }));

    const pad = (n) => String(n).padStart(2, "0");
    const toIso = (d) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
    const fromIso = (s) => {
        const [y, m, d] = (s || "").split("-").map(Number);
        return y && m && d ? new Date(y, m - 1, d) : null;
    };
    const addDays = (d, n) => new Date(d.getFullYear(), d.getMonth(), d.getDate() + n);
    const today = () => { const n = new Date(); return new Date(n.getFullYear(), n.getMonth(), n.getDate()); };
    const pretty = (s) => {
        const d = fromIso(s);
        return d ? d.toLocaleDateString(undefined, { weekday: "short", day: "numeric", month: "short" }) : "—";
    };

    // Text chips (location, source)
    form.querySelectorAll(".chip-row[data-fill]").forEach((row) => {
        const input = field(row.dataset.fill);
        const sync = () => row.querySelectorAll(".chip").forEach((c) =>
            c.classList.toggle("is-active", c.dataset.value.toLowerCase() === input.value.trim().toLowerCase()));
        row.addEventListener("click", (e) => {
            const chip = e.target.closest(".chip");
            if (!chip) return;
            input.value = chip.classList.contains("is-active") ? "" : chip.dataset.value;
            input.dataset.auto = "";
            fire(input);
        });
        input.addEventListener("input", sync);
        sync();
    });

    // Guess the source from the listing link
    const url = field("Url");
    const source = field("Source");
    const hosts = [
        ["linkedin.", "LinkedIn"], ["seek.", "Seek"], ["indeed.", "Indeed"],
        ["gradconnection.", "GradConnection"], ["prosple.", "Prosple"], ["glassdoor.", "Glassdoor"],
        ["workforceaustralia.", "Workforce Australia"], ["jora.", "Jora"],
    ];
    url.addEventListener("input", () => {
        let host = "";
        try { host = new URL(url.value.trim()).hostname.toLowerCase(); } catch { return; }
        if (source.value && !source.dataset.auto) return;
        const match = hosts.find(([h]) => host.includes(h));
        const guess = match ? match[1] : "Company site";
        if (source.value !== guess) {
            source.value = guess;
            source.dataset.auto = "1";
            fire(source);
        }
    });
    source.addEventListener("input", (e) => { if (e.isTrusted) source.dataset.auto = ""; });

    // Date chips
    const applied = field("DateApplied");
    const nextAction = field("NextActionDate");

    form.querySelectorAll(".chip-row[data-date-target]").forEach((row) => {
        const input = field(row.dataset.dateTarget);
        row.addEventListener("click", (e) => {
            const chip = e.target.closest(".chip");
            if (!chip) return;
            if (chip.hasAttribute("data-clear")) {
                input.value = "";
            } else {
                const base = row.dataset.relativeTo ? (fromIso(field(row.dataset.relativeTo).value) || today()) : today();
                input.value = toIso(addDays(base, Number(chip.dataset.offset)));
            }
            if (input === nextAction) nextAction.dataset.touched = "1";
            fire(input);
        });
    });

    // Keep the follow-up a week after the applied date until it's set by hand
    const appliedStart = fromIso(applied.value);
    if (appliedStart && nextAction.value && nextAction.value !== toIso(addDays(appliedStart, 7))) {
        nextAction.dataset.touched = "1";
    }
    nextAction.addEventListener("input", (e) => { if (e.isTrusted) nextAction.dataset.touched = "1"; });
    nextAction.addEventListener("change", () => { nextAction.dataset.touched = "1"; });
    applied.addEventListener("input", () => {
        const d = fromIso(applied.value);
        if (d && !nextAction.dataset.touched) {
            nextAction.value = toIso(addDays(d, 7));
            fire(nextAction);
        }
    });
    applied.addEventListener("change", () => fire(applied));

    // Word count for the job ad
    form.querySelectorAll("[data-count-for]").forEach((out) => {
        const input = field(out.dataset.countFor);
        const update = () => { out.textContent = (input.value.trim().match(/\S+/g) || []).length; };
        input.addEventListener("input", update);
        update();
    });

    // Live preview
    const cheers = {
        Applied: " Let's goooo",
        Screening: "Yo someone noticed you ",
        Interview: "Ow man interview time, good luck!",
        Offer: "Offer???? Damn we got options now",
        Accepted: "Can't say you're unemployed anymore huh",
        Rejected: "Another one bites the dust",
        Withdrawn: "Too good for them fs",
    };
    const statusBadge = form.querySelector("[data-preview-status]");
    const cheer = form.querySelector("[data-cheer]");

    const render = () => {
        form.querySelectorAll("[data-preview]").forEach((el) => {
            const value = field(el.dataset.preview).value.trim();
            el.textContent = value || el.dataset.empty;
            el.classList.toggle("is-empty", !value);
        });
        form.querySelectorAll("[data-preview-sep]").forEach((el) => {
            el.hidden = !field(el.dataset.previewSep).value.trim();
        });
        form.querySelectorAll("[data-preview-date]").forEach((el) => {
            el.textContent = pretty(field(el.dataset.previewDate).value);
        });
        const status = form.querySelector('[name="Status"]:checked')?.value || "Applied";
        statusBadge.className = `badge status-${status}`;
        statusBadge.textContent = status;
        cheer.textContent = cheers[status] || "";
    };

    form.addEventListener("input", render);
    form.addEventListener("change", render);
    render();

    // Ctrl/Cmd + Enter saves
    form.addEventListener("keydown", (e) => {
        if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
            e.preventDefault();
            form.requestSubmit();
        }
    });
})();
