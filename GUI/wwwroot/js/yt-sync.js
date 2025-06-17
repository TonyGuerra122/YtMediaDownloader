window.syncVideoAndAudio = (videoId, audioId) => {
    const video = document.getElementById(videoId);
    const audio = document.getElementById(audioId);

    if (!video || !audio) return;

    video.addEventListener('play', () => {
        if (audio.paused) audio.play();
    });

    video.addEventListener('pause', () => {
        if (!audio.paused) audio.pause();
    });

    video.addEventListener('seeked', () => {
        audio.currentTime = video.currentTime;
    });

    setInterval(() => {
        const diff = Math.abs(video.currentTime - audio.currentTime);
        if (diff > 0.1) {
            audio.currentTime = video.currentTime;
        };
    }, 1000);
}