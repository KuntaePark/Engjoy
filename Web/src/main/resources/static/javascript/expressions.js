import flatpickr from 'https://unpkg.com/flatpickr?module';
import 'https://unpkg.com/flatpickr/dist/l10n/ko.js';

document.addEventListener('DOMContentLoaded', () => {

            // 요소 가져오기 (한 곳에서 유일하게 선언)
            const searchForm = document.getElementById('search-form');
            const expressionContainer = document.getElementById('expression-container');
            const loadMoreBtn = document.getElementById('load-more-btn');
            const detailModalOverlay = document.getElementById('detail-modal-overlay');
            const recoModalOverlay = document.getElementById('reco-modal-overlay');
            const sortInput = document.getElementById('sort-input');
            const typeInput = document.getElementById('type-input');
            const startDateInput = document.getElementById('start-date');
            const endDateInput = document.getElementById('end-date');
            const sortBtn = document.getElementById('sort-btn');
            const sortDropdown = document.getElementById('sort-dropdown');
            const filterBtn = document.getElementById('filter-btn');
            const filterDropdown = document.getElementById('filter-dropdown');
            const typeSelect = document.getElementById('exprType');
            const resetBtn = document.getElementById('reset-btn');
            const alphaCheckbox = document.getElementById('alpha-sort-checkbox');
            const keywordInput = document.getElementById('keyword');
            const datePickerInput = document.getElementById('date-range-picker');

             console.log('datePickerInput 변수의 값:', datePickerInput);

            // 상태 관리 변수
            let currentPage = 0, isLastPage = false, isLoading = false;
            let currentPlayingAudio = null;
            let wordInfoData = {};
            let currentView = 'study_log';
            let datePickerInstance = null;
            let onlyFavorites = false;


            // 이벤트: 알파벳순 체크박스 (사전 모드일 때만)
              alphaCheckbox?.addEventListener('change', () => {
                if (alphaCheckbox.checked) {
                  sortInput.value = 'wordText,asc';
                } else {
                  sortInput.value = 'id,desc';
                }
                // 사전 모드인지 확인
                if (expressionContainer.classList.contains('grid')) {
                  applyFiltersToCurrentView();
                }
              });

             // 필터/정렬 적용 함수
              const applyFiltersToCurrentView = () => {
                // 현재 뷰 판별: grid 레이아웃이면 사전, 아니면 학습 기록
                const view = expressionContainer.classList.contains('grid')
                           ? 'dictionary'
                           : 'study_log';
                fetchAndRender({ isNewSearch: true, view });
              };

            // 함수 정의

            function initializePage() {
                const flatpickrInstance = initializeFlatpickr();
                setupEventListeners(flatpickrInstance);
                fetchWordInfo();
                fetchAndRender({ isNewSearch: true, view: 'study_log' });
                fetchAndShowRecommendations();
            }

                function setupEventListeners(flatpickrInstance) {

                    // --- 드롭다운 관련 요소들 ---
                    const typeBtn = document.getElementById('type-btn');
                    const typeDropdown = document.getElementById('type-dropdown');
                    const typeBtnLabel = document.getElementById('type-btn-label');

                    // --- 드롭다운 제어 ---
                    const closeAllDropdowns = () => {
                        sortDropdown?.classList.add('hidden');
                        filterDropdown?.classList.add('hidden');
                        typeDropdown?.classList.add('hidden');
                    };

                    window.addEventListener('click', (e) => {
                            if (e.target.closest('.flatpickr-calendar') || e.target.closest('.dropdown-content')) {
                                return;
                            }
                        closeAllDropdowns();
                    });

                    sortBtn?.addEventListener('click', e => {
                        e.stopPropagation();
                        sortDropdown.classList.toggle('hidden');
                        filterDropdown.classList.add('hidden');
                        typeDropdown.classList.add('hidden');
                    });

                    filterBtn?.addEventListener('click', e => {
                        e.stopPropagation();
                        filterDropdown.classList.toggle('hidden');
                        sortDropdown?.classList.add('hidden');
                        typeDropdown?.classList.add('hidden');
                    });

                    typeBtn?.addEventListener('click', e => {
                        e.stopPropagation();
                        typeDropdown.classList.toggle('hidden');
                        sortDropdown.classList.add('hidden');
                        filterDropdown.classList.add('hidden');
                    });


                    // 1) 뷰 전환할 때 currentView 갱신
                    document.getElementById('home-btn')?.addEventListener('click', () => {
                      currentView = 'study_log';
                      fetchAndRender({ isNewSearch: true, view: currentView });
                    });

                    document.getElementById('dictionary-btn')?.addEventListener('click', () => {
                      currentView = 'dictionary';             // ← 추가!
                      fetchAndRender({ isNewSearch: true, view: currentView });
                      closeAllDropdowns();
                    });


                    // 2) reset 버튼 핸들러
                    resetBtn?.addEventListener('click', () => {
                      if (searchForm) searchForm.reset();
                      sortInput.value = 'id,desc';
                      typeInput.value = '';
                      startDateInput.value = '';
                      endDateInput.value = '';
                      if (flatpickrInstance) flatpickrInstance.clear();

                      updateButtonState(sortBtn,   '정렬',  false);
                      updateButtonState(filterBtn, '날짜',  false);
                      updateButtonState(typeBtn,   '전체',  false);

                      if(alphaCheckbox){
                        alphaCheckbox.checked = false;
                      }

                      onlyFavorites = false;
                      currentView = 'study_log';

                      // 현재 뷰(currentView)에 맞춰 다시 그려주기
                      fetchAndRender({ isNewSearch: true, view: currentView });
                    });


                    // --- 정렬, 타입, 날짜 옵션 선택 ---
                    document.querySelectorAll('.sort-option').forEach(option => {
                        option.addEventListener('click', e => {
                            e.preventDefault();
                            sortInput.value = option.dataset.sort;
                            updateButtonState(sortBtn, option.innerText, true);
                            applyFiltersToCurrentView();
                            closeAllDropdowns();
                        });
                    });

                    document.querySelectorAll('.type-option').forEach(option => {
                        option.addEventListener('click', e => {
                            e.preventDefault();
                            const value = option.dataset.value;
                            typeInput.value = value;
                            onlyFavorites = (value == 'FAVORITE')
                            updateButtonState(typeBtn, option.textContent, true);
                            applyFiltersToCurrentView();
                            closeAllDropdowns();
                        });
                    });

                    document.querySelectorAll('.date-filter-option').forEach(link => {
                        link.addEventListener('click', e => {
                            e.preventDefault();
                            updateButtonState(filterBtn, link.innerText, true);
                            applyDateFilter(link.dataset.range, link.innerText, flatpickrInstance);
                            applyFiltersToCurrentView();
                            flatpickrInstance?.close();
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

                    // 모달 닫기
                    document.querySelectorAll('.close-modal-btn').forEach(btn => btn.addEventListener('click', closeAllModals));
                    detailModalOverlay?.addEventListener('click', e => { if(e.target===detailModalOverlay) closeAllModals(); });
                    recoModalOverlay?.addEventListener('click', e => { if(e.target===recoModalOverlay) closeAllModals(); });
                    document.getElementById('reco-close-btn')?.addEventListener('click', e => {
                        e.stopPropagation();
                        if(document.getElementById('reco-hide-checkbox')?.checked) setCookie('hideRecoModal','done',1);
                        closeAllModals();
                    });
                }

            function updateButtonState(btnElement, label, isActive) {
              if (!btnElement) return;
              // 1) 텍스트+아이콘 갱신
              btnElement.innerHTML = `
                <span>${label}</span>
                <i class="fa-solid fa-chevron-down text-sm ml-2"></i>
              `;
              // 2) 활성화 스타일 토글
              btnElement.classList.toggle('border-blue-500', isActive);
              btnElement.classList.toggle('text-blue-600',   isActive);
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
                            updateButtonState(filterBtn, '기간', true);
                            applyFiltersToCurrentView();
                        }
                    }
                });
            }

            function renderDictionary(data) {
              expressionContainer.className = 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6';

              // 사전 모드 응답이 { content: […] } 형태가 맞다면
              const items = Array.isArray(data.content) ? data.content : [];
              items.forEach(expr => {
                const merged = { ...expr, ...(wordInfoData[expr.exprId] || {}) };
                expressionContainer.insertAdjacentHTML('beforeend', createExpressionCard(merged));
              });
            }

            function renderStudyLog(data) {
              expressionContainer.className = 'space-y-8';

              Object.entries(data).forEach(([date, list]) => {
                let datePanel = expressionContainer.querySelector(`.date-panel[data-date="${date}"]`);
                if (!datePanel) {
                  datePanel = document.createElement('div');
                  datePanel.className = 'date-panel';
                  datePanel.dataset.date = date;
                  datePanel.innerHTML = `
                    <div class="flex items-center gap-2 mb-2">
                      <i class="fa-regular fa-calendar-check text-green-700"></i>
                      <h3 class="text-lg font-semibold text-gray-700">${date}</h3>
                    </div>
                    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6"></div>
                  `;
                  expressionContainer.appendChild(datePanel);
                }
                const grid = datePanel.querySelector('div.grid');
                list.forEach(expr => {
                  const merged = { ...expr, ...(wordInfoData[expr.exprId] || {}) };
                  grid.insertAdjacentHTML('beforeend', createExpressionCard(merged));
                });
              });
            }



            function fetchAndRender({ isNewSearch = false, view = 'study_log' } = {}) {
              // --- 1) 뷰 모드에 따라 UI 토글 ---
              const sortBtnContainer    = sortBtn?.parentElement;
              const filterBtnContainer  = filterBtn?.parentElement;
              const typeSelectContainer = typeSelect?.parentElement;
              const alphaContainer = document.getElementById('alpha-container');


              if (view === 'dictionary') {
                if (sortBtnContainer)    sortBtnContainer.style.display    = 'none';
                if (filterBtnContainer)  filterBtnContainer.style.display  = 'none';
                if (alphaContainer)      alphaContainer.style.display      = 'flex';

                expressionContainer.className = 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6';
              } else {
                if (sortBtnContainer)    sortBtnContainer.style.display    = '';
                if (filterBtnContainer)  filterBtnContainer.style.display  = '';
                if (alphaContainer)      alphaContainer.style.display      = 'none';

                expressionContainer.className = 'space-y-8';
              }

              if (isLoading || (!isNewSearch && isLastPage)) return;
                isLoading = true;
                loadMoreBtn && (loadMoreBtn.innerText = '로딩 중...');

                if (isNewSearch) {
                  currentPage = 0;
                  isLastPage  = false;
                  expressionContainer.innerHTML = '';
                  const oldMsg = document.getElementById('end-of-content-msg');
                  if(oldMsg) oldMsg.remove();
                }

                const params = new URLSearchParams({ view, page: currentPage });
                params.set('exprType',  typeInput.value);
                params.set('startDate', startDateInput.value);
                params.set('endDate',   endDateInput.value);
                params.set('keyword',   keywordInput.value);
                params.set('sort',      sortInput.value);

                fetch(`/expressions/api?${params.toString()}`)
                  .then(res => {
                            if (res.status === 401) {
                              // 인증이 안 된 경우
                              displayMessage("로그인 후 이용해주세요.");
                              return Promise.reject(new Error("Unauthorized"));
                            }
                            if (!res.ok) {
                              // 그 외 에러
                              return Promise.reject(new Error(`HTTP ${res.status}`));
                            }
                            // 정상 응답: JSON 파싱
                            return res.json();
                   })
                  .then(rawData => {
                    const isDictionaryView = view === 'dictionary';

                    // ★ 즐겨찾기 모드일 경우, 받아온 rawData를 실제로 필터링
                      if (onlyFavorites) {
                        if (isDictionaryView) {
                          // 사전 뷰: content 배열에서 favorite=true 만 남김
                          rawData.content = (rawData.content || [])
                            .filter(item => item.favorite);
                        } else {
                          // 학습 기록 뷰: 날짜별로 리스트를 걸러, 빈 날짜는 삭제
                          Object.entries(rawData).forEach(([date, list]) => {
                            const filtered = list.filter(item => item.favorite);
                            if (filtered.length) rawData[date] = filtered;
                            else delete rawData[date];
                          });
                        }
                      }

                    const isNowEmpty = isDictionaryView
                        ? !rawData.content || rawData.content.length === 0
                        : Object.keys(rawData).length === 0;

                    if (onlyFavorites && isNowEmpty) {
                         displayMessage("즐겨찾기한 기록이 없습니다.");
                         loadMoreBtn?.classList.add('hidden');
                         return;  // 더 이상 렌더링하지 않음
                    }

                    if (isNewSearch && isNowEmpty) {
                        const message = keywordInput.value.trim() ? "해당하는 단어나 문장이 존재하지 않습니다." : "해당하는 학습 기록이 없습니다.";
                        displayMessage(message);
                        loadMoreBtn?.classList.add('hidden');
                        return;
                    }

                     // ★ 필터링된 rawData를 렌더링
                      if (isDictionaryView) {
                        renderDictionary(rawData);
                        isLastPage = !!rawData.last;
                      } else {
                        renderStudyLog(rawData);
                        isLastPage = Object.keys(rawData).length === 0;
                      }

                    //  '더보기'를 눌렀는데 결과가 없는 경우
                    if (!isNewSearch && isNowEmpty) {
                        showEndOfContentMessage("더 이상 불러올 학습 기록이 없습니다.");
                        loadMoreBtn?.classList.add('hidden');
                        isLastPage = true; // 마지막 페이지라고 확정
                        return;
                    }


                    // 결과가 있는 경우, 기존 로직대로 화면을 렌더링
                    if (isDictionaryView) {
                        renderDictionary(rawData);
                        isLastPage = !!rawData.last;
                    } else {
                        renderStudyLog(rawData);
                        isLastPage = Object.keys(rawData).length === 0;
                    }

                    if (!isLastPage) {
                        currentPage++;
                        loadMoreBtn?.classList.remove('hidden');
                    } else {
                        loadMoreBtn?.classList.add('hidden');
                        showEndOfContentMessage("더 이상 불러올 학습 기록이 없습니다.");
                    }
                })
                .catch(err => {
                    console.error(err);
                    if (err.message !== 'Unauthorized') {
                          displayMessage("데이터를 불러오는 중 오류가 발생했습니다.");
                    }
                })
                .finally(() => {
                    isLoading = false;
                    if(loadMoreBtn) loadMoreBtn.innerText = '더보기';
                });
              }

              function displayMessage(message) {
                  if (!expressionContainer) return;
                  expressionContainer.innerHTML = `
                      <div class="text-center text-gray-500 py-20">
                          <i class="fa-regular fa-folder-open fa-3x mb-4"></i>
                          <p>${message}</p>
                      </div>
                  `;
              }

              function showEndOfContentMessage(message) {
                  // 이미 메시지가 있다면 중복으로 추가하지 않음
                  if (document.getElementById('end-of-content-msg')) return;

                  const messageEl = document.createElement('p');
                  messageEl.id = 'end-of-content-msg'; // 중복 방지를 위한 id
                  messageEl.className = 'text-center text-gray-500 py-10';
                  messageEl.textContent = message;

                  // '더보기' 버튼이 있다면, 그 바로 위에 메시지를 삽입
                  if (loadMoreBtn) {
                      loadMoreBtn.parentElement.insertBefore(messageEl, loadMoreBtn);
                  }
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
                return;
              }

                // 학습 기록 뷰: 날짜별 패널 + 그리드
                expressionContainer.className = 'space-y-8';

                Object.entries(data).forEach(([date, list]) => {
                  // 1) 동일 날짜 패널이 이미 있나 검사
                  let datePanel = expressionContainer.querySelector(`.date-panel[data-date="${date}"]`);

                  // 2) 없으면 새로 생성
                  if (!datePanel) {
                    datePanel = document.createElement('div');
                    datePanel.className = 'date-panel';
                    datePanel.dataset.date = date;  // ← 고유 식별자
                    datePanel.innerHTML = `
                      <div class="flex items-center gap-2 mb-2">
                        <i class="fa-regular fa-calendar-check text-green-700"></i>
                        <h3 class="text-lg font-semibold text-gray-700">${date}</h3>
                      </div>
                      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6"></div>
                    `;
                    expressionContainer.appendChild(datePanel);
                  }

                  // 3) 해당 패널의 그리드에 카드 추가
                  const grid = datePanel.querySelector('div.grid');
                  list.forEach(expr => {
                    const merged = { ...expr, ...(wordInfoData[expr.exprId] || {}) };
                    grid.insertAdjacentHTML('beforeend', createExpressionCard(merged));
                  });
                });
              }


            function fetchWordInfo() {
                 fetch('/wordinfo/wordinfo.json')
                    .then(res => res.ok ? res.json() : Promise.reject('wordinfo 로드 실패'))
                    .then(data => {
                        wordInfoData = data.reduce((acc, item) => {
                            if(item.exprId) acc[item.exprId] = item;
                            return acc;
                        }, {});
                    })
                    .catch(console.error)

            }

            function createExpressionCard(expr) {
                const detailInfo = {
                    part_of_speech: expr.part_of_speech,
                    synonyms: expr.synonyms,
                    antonyms: expr.antonyms,
                    collocations: expr.collocations,
                };
                const detailButtonHtml = (expr.exprType === 'WORD' && detailInfo.part_of_speech)
                  ? `<button class="detail-button absolute bottom-4 right-4 bg-green-600 hover:bg-green-500 text-white text-xs font-bold py-2 px-3 rounded-full" data-word-info='${JSON.stringify(detailInfo)}'>자세히</button>`
                  : '';
                return `
                <div class="word-card-wrapper">
                    <div class="word-card-inner">
                        <div class="word-card-front p-4 border border-gray-300 font-['Open_Sans']">
                            <div class="w-full flex justify-between items-center absolute top-4 left-0 px-4">
                                <span class="pron-audio-icon text-lg" data-audio-src="${expr.pronAudio}"><i class="fa-solid fa-volume-high"></i></span>
                                <span class="favorite-button text-lg" data-expr-id="${expr.exprId}"><i class="fa-star favorite-icon ${expr.favorite ? 'fa-solid active' : 'fa-regular'}"></i></span>
                                <div class="text-sm text-gray-500">*${expr.difficulty}</div>
                            </div>
                            <h3 class="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-full px-4 text-4xl font-bold text-gray-900">${expr.wordText}</h3>
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
            }

            // --- 페이지 실행 ---
            initializePage();
        });