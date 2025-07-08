import flatpickr from 'https://unpkg.com/flatpickr?module';
import 'https://unpkg.com/flatpickr/dist/l10n/ko.js';

document.addEventListener('DOMContentLoaded', () => {
            flatpickr("#customDateRangePicker", {
                mode: "range",
                dateFormat: "Y-m-d",
                locale: flatpickr.l10ns.ko
              });

            // --- 1. 요소 가져오기 (한 곳에서 유일하게 선언) ---
            const searchForm = document.getElementById('search-form');
            const expressionContainer = document.getElementById('expression-container');
            const loadMoreBtn = document.getElementById('load-more-btn');
            const detailModalOverlay = document.getElementById('detail-modal-overlay');
            const recoModalOverlay = document.getElementById('reco-modal-overlay');
            const wrongAnswerModal = document.getElementById('wrong-answer-modal');
            const wrongAnswerList = document.getElementById('wrong-answer-list');
            const sortInput = document.getElementById('sort-input');
            const typeInput = document.getElementById('type-input');
            const startDateInput = document.getElementById('start-date');
            const endDateInput = document.getElementById('end-date');
            const sortBtn = document.getElementById('sort-btn');
            const sortDropdown = document.getElementById('sort-dropdown');
            const filterBtn = document.getElementById('filter-btn');
            const filterDropdown = document.getElementById('filter-dropdown');
            const typeSelect = document.getElementById('exprType');
            const activeFiltersContainer = document.getElementById('active-filters');
            const resetBtn = document.getElementById('reset-btn');

            // --- 2. 상태 관리 변수 ---
            let currentPage = 0, isLastPage = false, isLoading = false;
            let currentPlayingAudio = null;
            let wordInfoData = {};
            let wrongAnswerData = [];


             // 필터/정렬 적용 함수
              const applyFiltersToCurrentView = () => {
                // 현재 뷰 판별: grid 레이아웃이면 사전, 아니면 학습 기록
                const view = expressionContainer.classList.contains('grid')
                           ? 'dictionary'
                           : 'study_log';
                fetchAndRender({ isNewSearch: true, view });
              };

            // --- 3. 함수 정의 ---

            function initializePage() {
                const flatpickrInstance = initializeFlatpickr();
                setupEventListeners(flatpickrInstance);
                fetchWordInfoAndRender();
            }

                function setupEventListeners(flatpickrInstance) {
                    // 드롭다운 제어
                    const closeAllDropdowns = () => {
                        sortDropdown.classList.add('hidden');
                        filterDropdown.classList.add('hidden');
                    };
                    sortBtn?.addEventListener('click', e => {
                        e.stopPropagation();
                        sortDropdown.classList.toggle('hidden');
                        filterDropdown.classList.add('hidden');
                    });
                    filterBtn?.addEventListener('click', e => {
                        e.stopPropagation();
                        filterDropdown.classList.toggle('hidden');
                        sortDropdown.classList.add('hidden');
                    });
                    window.addEventListener('click', closeAllDropdowns);

                    // 뷰 전환 버튼
                    document.getElementById('dictionary-btn')?.addEventListener('click', () => {
                        fetchAndRender({ isNewSearch: true, view: 'dictionary' });
                        closeAllDropdowns();
                    });
                    resetBtn?.addEventListener('click', () => {
                        if (searchForm) searchForm.reset();
                        sortInput.value = 'id,desc';
                        typeInput.value = '';
                        startDateInput.value = '';
                        endDateInput.value = '';
                        if (flatpickrInstance) flatpickrInstance.clear();
                        activeFiltersContainer.innerHTML = '';
                        updateSortButtonState(false);
                        typeSelect.value = '';
                        fetchAndRender({ isNewSearch: true, view: 'study_log' });
                    });

                    // 정렬 옵션
                    document.querySelectorAll('.sort-option').forEach(option => {
                        option.addEventListener('click', e => {
                            e.preventDefault();
                            sortInput.value = option.dataset.sort;
                            updateSortButtonState(true, option.innerText);
                            applyFiltersToCurrentView();
                            closeAllDropdowns();
                        });
                    });

                    // 타입 필터
                    typeSelect?.addEventListener('change', () => {
                        typeInput.value = typeSelect.value;
                        applyTypeFilter(typeSelect.options[typeSelect.selectedIndex].text);
                        applyFiltersToCurrentView();
                    });

                    // 날짜 필터
                    document.querySelectorAll('.date-filter-option').forEach(link => {
                        link.addEventListener('click', e => {
                            e.preventDefault();
                            applyDateFilter(link.dataset.range, link.innerText, flatpickrInstance);
                            applyFiltersToCurrentView();
                            closeAllDropdowns();
                        });
                    });

                    // 검색
                    searchForm?.addEventListener('submit', e => {
                        e.preventDefault();
                        applyFiltersToCurrentView();
                    });

                    // 더보기
                    loadMoreBtn?.addEventListener('click', () => {
                        const view = expressionContainer.classList.contains('grid') ? 'dictionary' : 'study_log';
                        fetchAndRender({ isNewSearch: false, view });
                    });

                    // 카드 클릭 (뒤집기, 발음, 즐겨찾기, 상세)
                    expressionContainer?.addEventListener('click', handleCardClick);

                    document.getElementById('wrong-answer-btn')?.addEventListener('click', openWrongAnswerModal);


                    // 모달 닫기
                    document.querySelectorAll('.close-modal-btn').forEach(btn => btn.addEventListener('click', closeAllModals));
                    detailModalOverlay?.addEventListener('click', e => { if(e.target===detailModalOverlay) closeAllModals(); });
                    recoModalOverlay?.addEventListener('click', e => { if(e.target===recoModalOverlay) closeAllModals(); });
                    wrongAnswerModal?.addEventListener('click', e => { if(e.target===wrongAnswerModal) closeAllModals(); });
                    document.getElementById('reco-close-btn')?.addEventListener('click', e => {
                        e.stopPropagation();
                        if(document.getElementById('reco-hide-checkbox')?.checked) setCookie('hideRecoModal','done',1);
                        closeAllModals();
                    });
                    wrongAnswerModal?.querySelectorAll('.wrong-sort-btn').forEach(btn => btn.addEventListener('click', () => {
                        wrongAnswerModal.querySelectorAll('.wrong-sort-btn').forEach(b => b.classList.replace('text-blue-600','text-gray-500'));
                        btn.classList.replace('text-gray-500','text-blue-600');
                        renderWrongAnswerTable(btn.dataset.sort);
                    }));
                }

            function addFilterBadge(type, label, onRemove) {
                if (!activeFiltersContainer) return;
                const existingBadge = activeFiltersContainer.querySelector(`[data-type="${type}"]`);
                if (existingBadge) existingBadge.remove();
                if (label === '타입 (전체)' && type === 'type') return;

                const badge = document.createElement('div');
                badge.className = 'flex items-center gap-1 bg-blue-100 text-blue-800 text-sm font-semibold px-3 py-1 rounded-full';
                badge.dataset.type = type;
                badge.innerHTML = `<span>${label}</span><button class="font-bold text-lg leading-none hover:text-blue-600 ml-1">&times;</button>`;

                badge.querySelector('button').onclick = () => {
                    onRemove();
                    badge.remove();
                    applyFiltersToCurrentView();
                };
                activeFiltersContainer.appendChild(badge);
            }

            function updateSortButtonState(isActive, text = '정렬') {
                if (!sortBtn) return;
                sortBtn.innerHTML = `${text} <i class="fa-solid fa-chevron-down text-xs ml-2"></i>`;
                if (isActive) {
                    sortBtn.classList.add('border-blue-500', 'border-2', 'font-semibold', 'text-blue-600');
                } else {
                    sortBtn.classList.remove('border-blue-500', 'border-2', 'font-semibold', 'text-blue-600');
                }
            }

            function applyTypeFilter(label) {
                if (typeInput.value) {
                    addFilterBadge("type", label, () => {
                        typeInput.value = '';
                        if(typeSelect) typeSelect.value = '';
                    });
                } else {
                    const existingBadge = activeFiltersContainer.querySelector(`[data-type="type"]`);
                    if (existingBadge) existingBadge.remove();
                }
            }

            function applyDateFilter(range, label, flatpickrInstance) {
                const today = new Date();
                let startDate = new Date();
                if (range.includes('~')) {
                    const [startStr,endStr] = range.split('~').map(s => s.trim());
                    startDateInput.value = startStr;
                    endDateInput.value = endStr;
                } else {
                    if (range === 'TODAY') startDate = today;
                    else if (range === 'LAST_WEEK') startDate.setDate(today.getDate() - 6);
                    else if (range === 'LAST_MONTH') startDate.setMonth(today.getMonth() - 1);
                    const toYYYYMMDD = date => date.toISOString().slice(0, 10);
                    startDateInput.value = toYYYYMMDD(startDate);
                    endDateInput.value = toYYYYMMDD(today);
                }
                addFilterBadge("dateRange", label, () => {
                    startDateInput.value = '';
                    endDateInput.value = '';
                    if (flatpickrInstance) flatpickrInstance.clear(false);
                });
            }

            function initializeFlatpickr() {
                const picker = document.getElementById('date-range-picker');
                if(!picker) return null;
                return flatpickr(picker, {
                    mode: "range", dateFormat: "Y-m-d", locale: "ko",
                    onClose: function (selectedDates, dateStr, instance) {
                        if (selectedDates.length === 2) {
                            const customRange = dateStr.replace(" to ", "~");
                            applyDateFilter(customRange, dateStr, instance);
                            applyFiltersToCurrentView();
                        }
                    }
                });
            }

            function fetchAndRender({ isNewSearch = false, view = 'study_log' } = {}) {
                if (isLoading || (!isNewSearch && isLastPage)) return;
                isLoading = true;
                if (loadMoreBtn) loadMoreBtn.innerText = '로딩 중...';

                if (isNewSearch) {
                    currentPage = 0;
                    isLastPage = false;
                    expressionContainer.innerHTML = '';
                    expressionContainer.className = view === 'dictionary'
                        ? 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6'
                        : 'space-y-8';
                }

                const params = new URLSearchParams({ view, page: currentPage });

                   // 날짜 & 타입 필터는 '학습 기록'에도, '사전'에도 공통으로 적용
                   params.set('exprType', typeInput.value);
                   params.set('startDate', startDateInput.value);
                   params.set('endDate', endDateInput.value);

                    // 검색어 모든 뷰에 공통 적용
                    params.set('keyword', document.getElementById('keyword').value);


                if (view === 'dictionary') {
                    params.set('sort', sortInput.value);
                } else {
                    params.set('sort', 'usedTime,desc');
                }



                fetch(`/expressions/api?${params.toString()}`)
                    .then(res => res.ok ? res.json() : promise.reject('데이터 로드 실패'))
                    .then(data => {
                        // 1) 화면에 렌더
                        renderData(view, data);

                        // 2) isLastPage 판정
                        if (view === 'dictionary') {
                          isLastPage = data.last;
                        } else {
                          // study_log 에는 빈 객체면 마지막 페이지로 간주
                          isLastPage = Object.keys(data).length === 0;
                        }

                        // 3) 다음 페이지가 있으면 currentPage 증가
                        if (!isLastPage) currentPage++;

                        // 4) 더보기 버튼 보이기/숨기기
                        if (loadMoreBtn) {
                          loadMoreBtn.classList.toggle('hidden', isLastPage);
                        }
                    })
                    .catch(console.error)
                    .finally(() => {
                        isLoading = false;
                        if(loadMoreBtn) loadMoreBtn.innerText = '더보기';
                    });
            }

            function renderData(view, data) {

              if (view === 'dictionary') {
                // 사전 뷰: 그리드 레이아웃
                expressionContainer.className = 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6';
                data.content.forEach(expr => {
                  // wordInfoData 머지
                  const merged = { ...expr, ...(wordInfoData[expr.exprId] || {}) };
                  expressionContainer.insertAdjacentHTML('beforeend', createExpressionCard(merged));
                });

              } else {
                // 학습 기록 뷰: 날짜별 스페이스 레이아웃
                expressionContainer.className = 'space-y-8';
                Object.entries(data).forEach(([date, list]) => {
                  const datePanel = document.createElement('div');
                  datePanel.className = 'date-panel';
                  datePanel.innerHTML = `
                    <div class="flex items-center gap-2 mb-2">
                      <i class="fa-regular fa-calendar-check text-green-700"></i>
                      <h3 class="text-lg font-semibold text-gray-700">${date}</h3>
                    </div>
                    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6"></div>
                  `;
                  const grid = datePanel.querySelector('div.grid');
                  list.forEach(expr => {
                    const merged = { ...expr, ...(wordInfoData[expr.exprId] || {}) };
                    grid.insertAdjacentHTML('beforeend', createExpressionCard(merged));
                  });
                  expressionContainer.appendChild(datePanel);
                });
              }
            }


            function fetchWordInfoAndRender() {
                 fetch('/wordinfo/wordinfo.json')
                    .then(res => res.ok ? res.json() : Promise.reject('wordinfo 로드 실패'))
                    .then(data => {
                        wordInfoData = data.reduce((acc, item) => {
                            if(item.exprId) acc[item.exprId] = item;
                            return acc;
                        }, {});
                    })
                    .catch(console.error)
                    .finally(() => {
                        fetchAndRender({ isNewSearch: true, view: 'study_log' });
                        fetchAndShowRecommendations();
                    });
            }

            async function openWrongAnswerModal() {
                if (!wrongAnswerModal) return;
                try {
                    const response = await fetch('/expressions/api/wrong-answers');
                    if (!response.ok) throw new Error('오답 목록 로딩 실패');
                    wrongAnswerData = await response.json();
                    renderWrongAnswerTable('recent');
                    wrongAnswerModal.classList.remove('hidden');
                } catch (error) {
                    console.error(error);
                    alert(error.message);
                }
            }

            function renderWrongAnswerTable(sortBy) {
                if (!wrongAnswerList) return;
                const sortedData = [...wrongAnswerData].sort((a, b) => {
                    if (sortBy === 'frequency') {
                        return b.incorrectCount - a.incorrectCount;
                    }
                    return new Date(b.lastReviewDate) - new Date(a.lastReviewDate);
                });
                wrongAnswerList.innerHTML = '';
                sortedData.forEach(item => {
                    const row = `
                        <tr class="bg-white border-b">
                            <td class="px-6 py-4 font-medium text-gray-900">${item.wordText}</td>
                            <td class="px-6 py-4">${item.meaning}</td>
                            <td class="px-6 py-4 text-center">${item.incorrectCount}</td>
                            <td class="px-6 py-4">${item.lastReviewDate}</td>
                        </tr>
                    `;
                    wrongAnswerList.insertAdjacentHTML('beforeend', row);
                });
            }

            function createExpressionCard(expr) {
                const detailInfo = {
                    part_of_speech: expr.part_of_speech,
                    synonyms: expr.synonyms,
                    antonyms: expr.antonyms,
                    collocations: expr.collocations,
                };
                const detailButtonHtml = (expr.exprType === 'WORD' && detailInfo.part_of_speech)
                  ? `<button class="detail-button absolute bottom-4 right-4 bg-green-500 hover:bg-green-600 text-white text-xs font-bold py-2 px-3 rounded-full" data-word-info='${JSON.stringify(detailInfo)}'>자세히</button>`
                  : '';
                return `
                <div class="word-card-wrapper">
                    <div class="word-card-inner">
                        <div class="word-card-front p-4">
                            <div class="w-full flex justify-between items-center absolute top-4 left-0 px-4">
                                <span class="pron-audio-icon text-2xl" data-audio-src="${expr.pronAudio}"><i class="fa-solid fa-volume-high"></i></span>
                                <span class="favorite-button text-2xl" data-expr-id="${expr.exprId}"><i class="fa-star favorite-icon ${expr.favorite ? 'fa-solid active' : 'fa-regular'}"></i></span>
                                <div class="text-sm text-gray-500">*${expr.difficulty}</div>
                            </div>
                            <h3 class="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-full px-4 text-3xl font-bold text-gray-900">${expr.wordText}</h3>
                        </div>
                        <div class="word-card-back p-6 relative">
                            <div class="flex flex-col justify-center h-full text-center">
                                <p class="text-gray-700 text-lg mb-3"><strong class="text-blue-600">뜻:</strong> ${expr.meaning}</p>
                                ${expr.exprType !== 'SENTENCE' ? `<p class="text-gray-600 text-md italic"><strong class="text-green-600">예문:</strong> ${expr.exp_sentence || 'N/A'}</p>` : '' }
                            </div>
                            ${detailButtonHtml}
                        </div>
                    </div>
                </div>`;
            }

            function handleCardClick(event) {
                const target = event.target;
                const detailButton = target.closest('.detail-button');
                const favoriteButton = target.closest('.favorite-button');
                const audioButton = target.closest('.pron-audio-icon');
                const cardWrapper = target.closest('.word-card-wrapper');

                if (detailButton) { event.stopPropagation(); openDetailModal(JSON.parse(detailButton.dataset.wordInfo)); }
                else if (favoriteButton) { event.stopPropagation(); toggleFavorite(favoriteButton); }
                else if (audioButton) { event.stopPropagation(); playAudio(audioButton.dataset.audioSrc); }
                else if (cardWrapper) { cardWrapper.classList.toggle('flipped'); }
            }

            function playAudio(src) {
                if (currentPlayingAudio) { currentPlayingAudio.pause(); }
                currentPlayingAudio = new Audio(src);
                currentPlayingAudio.play();
            }

            function toggleFavorite(button) {
                const exprId = button.dataset.exprId;
                const icon = button.querySelector('.favorite-icon');
                const token = document.querySelector("meta[name='_csrf']")?.content;
                const header = document.querySelector("meta[name='_csrf_header']")?.content;
                const headers = { 'Content-Type': 'application/json' };
                if (token && header) headers[header] = token;

                fetch(`/expressions/favorite/${exprId}`, { method: 'POST', headers: headers })
                    .then(res => res.ok ? res.json() : Promise.reject(`Favorite toggle failed: ${res.status}`))
                    .then(isFavorite => {
                        icon.classList.toggle('fa-solid', isFavorite);
                        icon.classList.toggle('fa-regular', !isFavorite);
                        icon.classList.toggle('active', isFavorite);
                    })
                    .catch(console.error);
            }

            function openDetailModal(wordInfo) {
                if(!detailModalOverlay) return;
                const dl = detailModalOverlay.querySelector('dl');

                // 값이 문자열이면 배열로 변환 후 join, 아니면 그대로 사용
                const formatToArrayString = (value) => {
                    if (typeof value === 'string' && value.length > 0) {
                        return value.split(',').map(s => s.trim()).join(', ');
                    }
                    if (Array.isArray(value)) {
                        return value.join(', ');
                    }
                    return 'N/A';
                };

                if (dl) {
                    dl.querySelector('#modal-part-of-speech').textContent = wordInfo.part_of_speech || 'N/A';
                    dl.querySelector('#modal-synonyms').textContent = formatToArrayString(wordInfo.synonyms);
                    dl.querySelector('#modal-antonyms').textContent = formatToArrayString(wordInfo.antonyms);
                    dl.querySelector('#modal-collocations').textContent = formatToArrayString(wordInfo.collocations);
                }
                detailModalOverlay.classList.remove('hidden');
            }

            function openRecoModal(recommendations) {
                if (!recoModalOverlay) return;
                const recoList = document.getElementById('reco-list');
                recoList.innerHTML = '';
                recommendations.forEach(expr => {
                    const li = document.createElement('li');
                    li.className = 'p-3 bg-gray-100 rounded-md';
                    li.innerText = `${expr.wordText} : ${expr.meaning}`;
                    recoList.appendChild(li);
                });
                recoModalOverlay.classList.remove('hidden');
            }

            function fetchAndShowRecommendations() {
                if (!recoModalOverlay || getCookie("hideRecoModal") === "done") return;
                fetch('/expressions/api/recommendations')
                    .then(res => res.ok ? res.json() : null)
                    .then(recommendations => {
                        if (recommendations && recommendations.length > 0) openRecoModal(recommendations);
                    }).catch(console.error);
            }
            function setCookie(name, value, days) {
                const date = new Date();
                date.setDate(date.getDate() + days);
                document.cookie = `${name}=${escape(value)}; path=/; expires=${date.toGMTString()}`;
            }
            function getCookie(name) {
                const cookies = document.cookie.split("; ");
                for (const cookie of cookies) {
                    const [key, val] = cookie.split("=");
                    if (key === name) return unescape(val);
                }
                return null;
            }
            function closeAllModals() {
                detailModalOverlay?.classList.add('hidden');
                recoModalOverlay?.classList.add('hidden');
                wrongAnswerModal?.classList.add('hidden');
            }

            // --- 페이지 실행 ---
            initializePage();
        });