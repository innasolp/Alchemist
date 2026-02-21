

function resumeSelectedJob(jobId) {
    fetch(`/api/jobs/${jobId}/resume`, { method: 'POST' }).
        then(() => loca.reload());
}

function pauseSelectedJob(jobId) {
    fetch(`/api/jobs/${jobId}/pause`, { method: 'POST' }).
        then(() => loca.reload());
}

(function () {
    function addControlButtons() {
        // Ищем строки в таблице задач (Jobs)
        const rows = document.querySelectorAll('.table tbody tr');

        rows.forEach(row => {
            const jobIdLink = row.querySelector('td a[href*="/jobs/details/"]');
            if (!jobIdLink || row.querySelector('.job-control-btn')) return;

            const jobId = jobIdLink.innerText.replace('#', '');
            const actionContainer = row.querySelector('td:last-child');

            if (actionContainer) {
                // Создаем кнопку Pause
                const pauseBtn = document.createElement('button');
                pauseBtn.className = 'btn btn-xs btn-warning job-control-btn';
                pauseBtn.innerText = 'Pause';
                pauseBtn.onclick = () => sendAction(jobId, 'pause');

                // Создаем кнопку Resume
                const resumeBtn = document.createElement('button');
                resumeBtn.className = 'btn btn-xs btn-success job-control-btn';
                resumeBtn.style.marginLeft = '5px';
                resumeBtn.innerText = 'Resume';
                resumeBtn.onclick = () => sendAction(jobId, 'resume');

                actionContainer.appendChild(pauseBtn);
                actionContainer.appendChild(resumeBtn);
            }
        });
    }

    function sendAction(jobId, action) {
        // Отправляем запрос на наш кастомный эндпоинт
        fetch(`/hangfire/job-control?jobId=${jobId}&action=${action}`, { method: 'POST' })
            .then(res => {
                if (res.ok) alert(`Job ${jobId} ${action}d successfully!`);
                else alert('Error: ' + res.statusText);
            });
    }

    // Запускаем при загрузке и каждые 2 секунды (т.к. Dashboard обновляется через AJAX)
    setInterval(addControlButtons, 2000);
})();